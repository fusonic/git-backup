using CliWrap;
using CliWrap.Buffered;
using Fusonic.GitBackup.Models;
using Microsoft.Extensions.Logging;

namespace Fusonic.GitBackup.Services;

internal class FullBackupStrategy : IBackupStrategy
{
    private readonly AppSettings settings;
    private readonly ILogger logger;
    private readonly string timestampFolderName;

    public FullBackupStrategy(AppSettings settings, ILogger logger)
    {
        this.settings = settings;
        this.logger = logger;
        timestampFolderName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
    }

    public async Task Backup(Repository repository)
    {
        await Cli.Wrap("git")
            .WithArguments($"clone --mirror {repository.HttpsUrl} {settings.Backup.Local.Destination}/{timestampFolderName}/{repository.Provider}/{repository.Name}")
            .WithValidation(CommandResultValidation.ZeroExitCode)
            .ExecuteBufferedAsync();
    }

    public Task Cleanup()
    {
        var deleteAfterDays = settings.Backup.Local.DeleteAfterDays;
        logger.LogInformation($"Deleting backups older than {deleteAfterDays}");

        return Task.Run(() =>
        {
            foreach (var dir in Directory.EnumerateDirectories(settings.Backup.Local.Destination)
                .Where(x => DateTime.Now - Directory.GetLastWriteTime(x)
                            > TimeSpan.FromDays(deleteAfterDays)))
            {
                Directory.Delete(dir, true);
            }

            logger.LogInformation("Old backups deleted");
        });
    }
}