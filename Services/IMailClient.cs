namespace Fusonic.GitBackup.Services;

public interface IMailClient
{
    void SendMail(string subject, string body);
}
