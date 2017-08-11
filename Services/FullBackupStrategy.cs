using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fusonic.GitBackup.Models;
using Microsoft.Extensions.Logging;

namespace Fusonic.GitBackup.Services
{
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

        public Task Backup(Repository repository)
        {
            return Task.Run(() =>
            {
                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = "git",
                        Arguments =
                            $@"clone --mirror {repository.HttpsUrl} {settings.Backup.Local.Destination}/{timestampFolderName}/{repository.Provider}/{repository.Name}",
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

        public Task Cleanup()
        {
            var deleteAfterDays = settings.Backup.Local.DeleteAfterDays;
            logger.LogInformation($"Deleting backups older than {deleteAfterDays}");

            return Task.Run(() =>
            {
                foreach (var dir in Directory.GetDirectories(settings.Backup.Local.Destination)
                    .Where(x => DateTime.Now - Directory.GetLastWriteTime(x)
                                > TimeSpan.FromDays(deleteAfterDays)))
                {
                    Directory.Delete(dir, true);
                }

                logger.LogInformation("Old backups deleted");
            });
        }
    }
}