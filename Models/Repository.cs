namespace Fusonic.GitBackup.Models 
{
    public class Repository
    {
        public string HttpsUrl {get; set;}
        public GitProvider Provider {get; set;}
        public string Name { get; set; }
        public string Username { get; set; }
    }
}