using System.Net.Http.Headers;
using Fusonic.GitBackup.Models;
using RestEase;

namespace Fusonic.GitBackup.Services.Api;

[Header("User-Agent", "FusonicGitBackup")]
public interface IBitbucketApi
{
    [Header("Authorization")]
    AuthenticationHeaderValue Authorization { get; set; }

    [Get("repositories")]
    Task<BitbucketRepository> GetRepositoriesAsync([RawQueryString] string after, [Query("role")] string role);

    [Get("repositories")]
    Task<BitbucketRepository> GetRepositoriesAsync([Query("role")] string role);
}
