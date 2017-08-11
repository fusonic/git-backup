using System.Threading.Tasks;

namespace Fusonic.GitBackup.Services.Heartbeat
{
    public interface IHeartbeat
    {
        Task Notify();
    }
}