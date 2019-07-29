namespace Fusonic.GitBackup.Models
{
    public class AppSettings
    {
        public GitSettings[] Git { get; set; }
        public BackupSettings Backup { get; set; }
        public MailSettings Mail { get; set; }
        public string DeadmanssnitchUrl { get; set; }
        
        public class GitSettings
        {
            public GitProvider Type { get; set; }
            public string Username { get; set; }
            public string PersonalAccessToken { get; set; }

            public override string ToString() => Type.ToString();
        }
        
        public class BackupSettings
        {
            public int MaxDegreeOfParallelism { get; set; }
            public LocalSettings Local { get; set; }
        }
        
        public class LocalSettings
        {
            public int DeleteAfterDays { get; set; }
            public string Destination { get; set; }
            public BackupStrategy Strategy { get; set; }

            public enum BackupStrategy
            {
                Incremental,
                Full
            }
        }
        
        public class MailSettings
        {
            public string Host { get; set; }
            public int Port { get; set; }
            public bool UseSsl { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public MailUserSettings Sender { get; set; }
            public MailUserSettings Receiver { get; set; }
        }
        
        public class MailUserSettings
        {
            public string Name { get; set; }
            public string Address { get; set; }
        }
    }
}