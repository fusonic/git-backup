namespace Fusonic.GitBackup.Services.Heartbeat;

public interface IHeartbeat
{
    Task Notify();
}
