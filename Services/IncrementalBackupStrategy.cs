using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Fusonic.GitBackup.Models;

namespace Fusonic.GitBackup.Services
{
    internal class IncrementalBackupStrategy : IBackupStrategy
    {
        private readonly AppSettings settings;

        public IncrementalBackupStrategy(AppSettings settings)
            => this.settings = settings;

        public Task Backup(Repository repository)
        {
            return Task.Run(() =>
            {
                var path = $"{settings.Backup.Local.Destination}/{repository.Provider}/{repository.Name}";
                var cmd = Directory.Exists(path)
                    ? $"--git-dir={path} remote update"
                    : $"clone --mirror {repository.HttpsUrl} {path}";

                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = "git",
                        Arguments = cmd,
                        RedirectStandardError = true,
                    }
                };

                process.StartInfo.Environment.Add("GIT_BACKUP_ACCESS_TOKEN", repository.PersonalAccessToken);
                process.StartInfo.Environment.Add("GIT_ASKPASS", Path.GetFullPath("git-askpass" + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".cmd" : ".sh")));

                process.Start();
                var errorOutput = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new Exception(errorOutput);
            });
        }

        public Task Cleanup() => Task.CompletedTask; // No cleanup when incrementally backuping
    }
}