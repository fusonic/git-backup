namespace Fusonic.GitBackup.Models;

public class Repository
{
    public string HttpsUrl { get; set; }
    public GitProvider Provider { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
    public string PersonalAccessToken { get; set; }

    public override string ToString()
        => $"{Provider.ToString()}: {Name}";
}
