namespace Fusonic.GitBackup.Services.Heartbeat;

internal class NullBeat : IHeartbeat
{
    public Task Notify() => Task.CompletedTask;
}
