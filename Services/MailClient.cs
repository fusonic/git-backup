using Fusonic.GitBackup.Models;
using MailKit.Net.Smtp;
using MimeKit;

namespace Fusonic.GitBackup.Services;

internal class MailClient : IMailClient
{
    private readonly AppSettings settings;
    private readonly SmtpClient client;

    public MailClient(AppSettings settings, SmtpClient client)
    {
        this.settings = settings;
        this.client = client;
    }

    public void SendMail(string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.Mail.Sender.Name, settings.Mail.Sender.Address));
        message.To.Add(new MailboxAddress(settings.Mail.Receiver.Name, settings.Mail.Receiver.Address));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        client.Send(message);
        client.Disconnect(true);
    }
}
