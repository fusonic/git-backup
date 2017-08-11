using System.Collections.Generic;
using System.Threading.Tasks;
using Fusonic.GitBackup.Models;
using RestEase;

namespace Fusonic.GitBackup.Services.Api
{
    [Header("User-Agent", "FusonicGitBackup")]
    public interface IGitlabApi
    {
        [Header("PRIVATE-TOKEN")]
        string PrivateToken { get; set; }
        
        [Get("projects")]
        Task<Response<List<GitlabRepository>>> GetRepositoriesAsync([RawQueryString] string perPage);
    }
}