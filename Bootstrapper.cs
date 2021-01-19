using System;
using Fusonic.GitBackup.Models;
using Fusonic.GitBackup.Services;
using Fusonic.GitBackup.Services.Api;
using Fusonic.GitBackup.Services.Git;
using Fusonic.GitBackup.Services.Heartbeat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestEase;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Fusonic.GitBackup
{
    internal static class Bootstrapper
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
            container.Collection.Register<IGitService>(new[]
            {
                typeof(BitbucketService),
                typeof(GitlabService),
                typeof(GithubService)
            });

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder.AddConsole());

            serviceCollection.AddSimpleInjector(container, action => action.AddLogging());

            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.UseSimpleInjector(container);

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("app-settings.json", false)
                .AddJsonFile("app-settings.overwrite.json", true);

            var configuration = builder.Build();

            var settings = new AppSettings();
            configuration.Bind(settings);
            container.RegisterInstance(settings);

            container.Register<Func<IGitlabApi>>(() => () => RestClient.For<IGitlabApi>("https://gitlab.com/api/v4"));
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