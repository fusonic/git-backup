using Newtonsoft.Json;

namespace Fusonic.GitBackup.Models;

public class GithubRepository
{
    [JsonProperty("clone_url")]
    public string HttpsUrl { get; set; }

    [JsonProperty("full_name")]
    public string Name { get; set; }
}
