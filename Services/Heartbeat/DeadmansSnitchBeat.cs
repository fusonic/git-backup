using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Fusonic.GitBackup.Services.Heartbeat
{
    internal class DeadmansSnitchBeat : IHeartbeat
    {
        private readonly string url;
        private readonly ILogger logger;

        public DeadmansSnitchBeat(string url, ILogger logger)
        {
            this.url = url;
            this.logger = logger;
        }

        public async Task Notify()
        {
            logger.LogInformation($"Sending Heartbeat to {url} ...");
            using (var client = new HttpClient())
            {
                await client.GetStringAsync(url);
            }
        }
    }
}