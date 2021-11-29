using Fusonic.GitBackup.Models;
using static Fusonic.GitBackup.Models.AppSettings;

namespace Fusonic.GitBackup.Services.Git;

public interface IGitService
{
    GitProvider Provider { get; }
    Task<List<Repository>> GetRepositoryUrisAsync(IEnumerable<GitSettings> settings);
}
