using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Fusonic.GitBackup.Models;
using Microsoft.Extensions.Logging;

namespace Fusonic.GitBackup.Services
{
    internal class IncrementalBackupStrategy : IBackupStrategy
    {
        private readonly AppSettings settings;
        private readonly ILogger logger;

        public IncrementalBackupStrategy(AppSettings settings, ILogger logger)
        {
            this.settings = settings;
            this.logger = logger;
        }

        public Task Backup(Repository repository)
        {
            return Task.Run(() =>
            {
                var path = $"{settings.Backup.Local.Destination}/{repository.Provider}/{repository.Name}";
                var cmd = Directory.Exists(path)
                    ? $"--git-dir={path} remote update"
                    : $@"clone --mirror {repository.HttpsUrl} {path}";

                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = "git",
                        Arguments = cmd,
                        RedirectStandardError = true
                    }
                };
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