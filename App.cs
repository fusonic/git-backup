using System.Threading.Tasks.Dataflow;
using Fusonic.GitBackup.Models;
using Fusonic.GitBackup.Services;
using Fusonic.GitBackup.Services.Git;
using Fusonic.GitBackup.Services.Heartbeat;
using Microsoft.Extensions.Logging;

namespace Fusonic.GitBackup;

internal class App
{
    private readonly IEnumerable<IGitService> gitServices;
    private readonly IBackupStrategy backupRunner;
    private readonly ILogger logger;
    private readonly IHeartbeat heartbeat;
    private readonly AppSettings settings;

    public App(IEnumerable<IGitService> gitServices, IBackupStrategy backupRunner, ILogger logger, IHeartbeat heartbeat, AppSettings settings)
    {
        this.gitServices = gitServices;
        this.backupRunner = backupRunner;
        this.logger = logger;
        this.heartbeat = heartbeat;
        this.settings = settings;
    }

    public async Task Run()
    {
        var current = 0;
        var pending = 0;
        var repositoryCount = 0;

        var fetchBlock = new TransformManyBlock<IGitService, Repository>(
         async service =>
         {
             var friendlyServiceName = service.Provider.ToString();
             logger.LogInformation($"Fetching repositories from {friendlyServiceName} ...");

             var providerSettings = settings.Git.Where(x => x.Type == service.Provider);
             var repositories = await service.GetRepositoryUrisAsync(providerSettings);
             if (providerSettings.Any() && repositories.Count == 0)
                 throw new InvalidOperationException($"{friendlyServiceName} API returned 0 repositories. Please check your credentials.");

             Interlocked.Add(ref repositoryCount, repositories.Count);
             logger.LogInformation($"Fetched {repositories.Count} repositories from {friendlyServiceName}. ({repositoryCount} total repositories to backup) ");

             return repositories;
         }, new ExecutionDataflowBlockOptions() { BoundedCapacity = 1000, MaxDegreeOfParallelism = settings.Backup.MaxDegreeOfParallelism });

        var mirrorBlock = new ActionBlock<Repository>(
            async repository =>
            {
                Interlocked.Increment(ref current);
                Interlocked.Increment(ref pending);

                logger.LogInformation($"Cloning {current} of {repositoryCount} ({pending} running) ({repository.HttpsUrl})");
                await backupRunner.Backup(repository);
                logger.LogInformation($"Finished {repository.HttpsUrl} ({pending} pending)");

                Interlocked.Decrement(ref pending);
            }, new ExecutionDataflowBlockOptions() { BoundedCapacity = 1000, MaxDegreeOfParallelism = settings.Backup.MaxDegreeOfParallelism, });


        fetchBlock.LinkTo(mirrorBlock, new DataflowLinkOptions() { PropagateCompletion = true });

        foreach (var service in gitServices)
        {
            await fetchBlock.SendAsync(service);
        }

        fetchBlock.Complete();
        await mirrorBlock.Completion;

        await backupRunner.Cleanup();
        await heartbeat.Notify();

        logger.LogInformation("Done.");
    }
}
