using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fusonic.GitBackup.Models;
using Fusonic.GitBackup.Services.Api;
using static Fusonic.GitBackup.Models.AppSettings;

namespace Fusonic.GitBackup.Services.Git
{
    internal class GithubService : IGitService
    {
        private readonly Func<IGithubApi> apiFactory;
        private readonly Regex regex = new Regex(@"<(.*)\?(.*)>; rel=""next""", RegexOptions.CultureInvariant);

        public GithubService(Func<IGithubApi> apiFactory) => this.apiFactory = apiFactory;

        public GitProvider Provider => GitProvider.Github;

        public async Task<List<Repository>> GetRepositoryUrisAsync(IEnumerable<GitSettings> settings)
        {
            var repositories = new List<Repository>();
            foreach (var gitSetting in settings)
            {
                var api = apiFactory();
                api.Authorization = new AuthenticationHeaderValue("Basic", TokenGenerator.GenerateBase64Token(gitSetting.Username, gitSetting.PersonalAccessToken));
                
                var nextPage = "?per_page=1000";
                while (!string.IsNullOrEmpty(nextPage))
                {
                    var response = await api.GetRepositoriesAsync(nextPage);

                    repositories.AddRange(response.GetContent().Select(x => new Repository()
                    {
                        HttpsUrl = x.HttpsUrl,
                        Provider = GitProvider.Github,
                        Name = x.Name,
                        Username = gitSetting.Username,
                        PersonalAccessToken = gitSetting.PersonalAccessToken
                    }));

                    if (response.ResponseMessage.Headers.Contains("Link"))
                    {
                        nextPage = response.ResponseMessage.Headers.GetValues("Link").FirstOrDefault();
                        var match = regex.Match(nextPage);
                        if (match.Success)
                            nextPage = "?" + match.Groups[2].Value;
                        else
                            nextPage = null;
                    }
                    else
                        nextPage = null;
                }
            }
            return repositories;
        }
    }
}