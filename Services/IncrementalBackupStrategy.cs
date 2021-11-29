using System.Runtime.InteropServices;
using CliWrap;
using CliWrap.Buffered;
using Fusonic.GitBackup.Models;

namespace Fusonic.GitBackup.Services;

internal class IncrementalBackupStrategy : IBackupStrategy
{
    private readonly AppSettings settings;

    public IncrementalBackupStrategy(AppSettings settings)
        => this.settings = settings;

    public async Task Backup(Repository repository)
    {
        var path = $"{settings.Backup.Local.Destination}/{repository.Provider}/{repository.Name}";
        var cmd = Directory.Exists(path)
            ? $"--git-dir={path} remote update"
            : $"clone --mirror {repository.HttpsUrl} {path}";

        var result = await Cli.Wrap("git")
            .WithArguments(cmd)
            .WithEnvironmentVariables(env => env
                .Set("GIT_BACKUP_ACCESS_TOKEN", repository.PersonalAccessToken)
                .Set("GIT_ASKPASS", Path.GetFullPath("git-askpass" + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".cmd" : ".sh"))))
            .ExecuteBufferedAsync();

        if (result.ExitCode != 0)
            throw new Exception(result.StandardError);
    }

    public Task Cleanup() => Task.CompletedTask; // No cleanup when incrementally backuping
}