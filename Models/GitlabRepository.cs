using Newtonsoft.Json;

namespace Fusonic.GitBackup.Models;

public class GitlabRepository
{
    [JsonProperty("http_url_to_repo")]
    public string HttpsUrl { get; set; }

    [JsonProperty("path_with_namespace")]
    public string Name { get; set; }

    [JsonProperty("default_branch")]
    public string DefaultBranch { get; set; }

    public override string ToString()
         => $"{Name}";
}
