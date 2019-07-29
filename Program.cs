using System;
using System.Threading.Tasks;
using Fusonic.GitBackup.Models;
using Fusonic.GitBackup.Services;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Fusonic.GitBackup
{
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
                SendErrorMail(ex);
                using (AsyncScopedLifestyle.BeginScope(container))
                {
                    container.GetService<ILogger>().LogError("An error occured. We sent an email with a detailed error message to the email defined in the app-settings.json.");
                }
            }
        }

        private static void SendErrorMail(Exception ex)
        {
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                var settings = container.GetService<AppSettings>();
                using (var client = new SmtpClient())
                {
                    client.Connect(settings.Mail.Host, settings.Mail.Port, settings.Mail.UseSsl);
                    client.Authenticate(settings.Mail.Username, settings.Mail.Password);

                    var mailClient = new MailClient(settings, client);
                    mailClient.SendMail("Git-Backup Fatal Error!", ex.ToString());
                }
            }
        }
    }
}