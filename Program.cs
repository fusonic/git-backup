using Fusonic.GitBackup.Models;
using Fusonic.GitBackup.Services;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Fusonic.GitBackup;

public static class Program
{
    private static Container container;

    public static void Main(string[] args) => Init().Wait();

    private static async Task Init()
    {
        try
        {
            container = Bootstrapper.CreateContainer();
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                await container.GetService<App>().Run();
            }
        }
        catch (Exception ex)
        {
            if (container == null) throw;
            var sentMail = SendErrorMail(ex);
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                var logger = container.GetService<ILogger>();
                if (sentMail)
                {
                    logger.LogError("An error occured. An email with detailed error message has been sent to the address defined in the app-settings.json.");
                }
                logger.LogError(ex.ToString());
            }
        }
    }

    private static bool SendErrorMail(Exception ex)
    {
        using (AsyncScopedLifestyle.BeginScope(container))
        {
            var settings = container.GetService<AppSettings>();
            if (string.IsNullOrEmpty(settings.Mail.Host))
                return false;

            using (var client = new SmtpClient())
            {
                client.Connect(settings.Mail.Host, settings.Mail.Port, settings.Mail.UseSsl);
                client.Authenticate(settings.Mail.Username, settings.Mail.Password);

                var mailClient = new MailClient(settings, client);
                mailClient.SendMail("Git-Backup Fatal Error!", ex.ToString());
                return true;
            }
        }
    }
}
