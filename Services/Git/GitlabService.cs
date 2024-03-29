using System.Text.RegularExpressions;
using Fusonic.GitBackup.Models;
using Fusonic.GitBackup.Services.Api;
using static Fusonic.GitBackup.Models.AppSettings;

namespace Fusonic.GitBackup.Services.Git;

internal class GitlabService : IGitService
{
    private readonly Func<IGitlabApi> apiFactory;
    private readonly Regex regex = new Regex(@"<(.*)\?(.*)>; rel=""next""", RegexOptions.CultureInvariant);

    public GitlabService(Func<IGitlabApi> apiFactory)
    {
        this.apiFactory = apiFactory;
    }

    public GitProvider Provider => GitProvider.Gitlab;

    public async Task<List<Repository>> GetRepositoryUrisAsync(IEnumerable<GitSettings> settings)
    {
        var repositories = new List<Repository>();
        foreach (var gitSetting in settings)
        {
            var api = apiFactory();
            api.PrivateToken = gitSetting.PersonalAccessToken;

            var nextPage = "?per_page=1000&membership=true";
            while (!string.IsNullOrEmpty(nextPage))
            {
                var response = await api.GetRepositoriesAsync(nextPage);
                var responseContent = response.GetContent();
                repositories.AddRange(responseContent
                    .Where(x => x.DefaultBranch != null)
                    .Select(x => new Repository()
                    {
                        HttpsUrl = x.HttpsUrl.Replace("//", "//gitlab-ci-token@"),
                        Provider = GitProvider.Gitlab,
                        Name = x.Name,
                        Username = gitSetting.Username,
                        PersonalAccessToken = gitSetting.PersonalAccessToken
                    }));

                nextPage = response.ResponseMessage.Headers.GetValues("Link").FirstOrDefault();
                var match = regex.Match(nextPage);
                if (match.Success)
                    nextPage = "?" + match.Groups[2].Value;
                else
                    nextPage = null;
            }
        }
        return repositories;
    }
}
