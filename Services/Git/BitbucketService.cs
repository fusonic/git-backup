using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Fusonic.GitBackup.Models;
using Fusonic.GitBackup.Services.Api;
using static Fusonic.GitBackup.Models.AppSettings;

namespace Fusonic.GitBackup.Services.Git
{
    internal class BitbucketService : IGitService
    {
        private readonly Func<IBitbucketApi> apiFactory;

        public BitbucketService(Func<IBitbucketApi> apiFactory) => this.apiFactory = apiFactory;

        public GitProvider Provider => GitProvider.Bitbucket;

        public async Task<List<Repository>> GetRepositoryUrisAsync(IEnumerable<GitSettings> settings)
        {
            var allRepositories = new List<Repository>();
            foreach (var gitSetting in settings)
            {
                var api = apiFactory();
                api.Authorization = new AuthenticationHeaderValue("Basic", TokenGenerator.GenerateBase64Token(gitSetting.Username, gitSetting.PersonalAccessToken));

                var after = "";
                while (after != null)
                {
                    BitbucketRepository resp;
                    if (after != "")
                        resp = await api.GetRepositoriesAsync(after, "member");
                    else
                        resp = await api.GetRepositoriesAsync("member");

                    allRepositories.AddRange(from v in resp.Values
                                             from c in v.Links.Clone
                                             where c.Name == "https"
                                             select new Repository()
                                             {
                                                 HttpsUrl = c.Href,
                                                 Provider = GitProvider.Bitbucket,
                                                 Name = v.Name,
                                                 Username = gitSetting.Username,
                                                 PersonalAccessToken = gitSetting.PersonalAccessToken
                                             });
                    if (resp.Next != null)
                        after = "?" + resp.Next.Split(new string[] { "?", "&" }, StringSplitOptions.None)[1];
                    else
                        after = null;
                }
            }
            return allRepositories;
        }
    }
}