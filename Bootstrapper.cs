﻿using System;
using Fusonic.GitBackup.Models;
using Fusonic.GitBackup.Services;
using Fusonic.GitBackup.Services.Api;
using Fusonic.GitBackup.Services.Git;
using Fusonic.GitBackup.Services.Heartbeat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestEase;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Fusonic.GitBackup
{
    static class Bootstrapper
    {
        internal static Container CreateContainer()
        {
            var container = new Container()
            {
                Options =
                {
                    DefaultScopedLifestyle = new AsyncScopedLifestyle(),
                    DefaultLifestyle = Lifestyle.Scoped,
                }
            };
            container.RegisterCollection<IGitService>(new[]
            {
                typeof(BitbucketService),
                typeof(GitlabService),
                typeof(GithubService)
            });

            var loggerBuilder = new LoggerFactory()
                 .AddConsole()
                 .AddDebug()
                 .CreateLogger("globalLogger");
            container.RegisterSingleton(loggerBuilder);

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("app-settings.json", false)
                .AddJsonFile("app-settings.overwrite.json", true);

            var configuration = builder.Build();

            var settings = new AppSettings();
            configuration.Bind(settings);
            container.RegisterSingleton(settings);

            container.Register<Func<IGitlabApi>>(() => () => RestClient.For<IGitlabApi>("https://gitlab.com/api/v3"));
            container.Register<Func<IGithubApi>>(() => () => RestClient.For<IGithubApi>("https://api.github.com"));
            container.Register<Func<IBitbucketApi>>(() => () => RestClient.For<IBitbucketApi>("https://api.bitbucket.org/2.0"));
            
            container.Register(typeof(IHeartbeat), () =>
                !string.IsNullOrEmpty(settings.DeadmanssnitchUrl)
                    ? new DeadmansSnitchBeat(settings.DeadmanssnitchUrl, container.GetInstance<ILogger>())
                    : (IHeartbeat)new NullBeat());

            container.Register(typeof(IBackupStrategy),
                settings.Backup.Local.Strategy == AppSettings.LocalSettings.BackupStrategy.Full
                    ? typeof(FullBackupStrategy)
                    : typeof(IncrementalBackupStrategy));

            container.Register<App>();
            container.Verify();
            return container;
        }
    }
}