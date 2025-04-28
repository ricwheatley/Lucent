namespace Lucent.Scheduler;

public interface IRunNow
{
    Task RunAsync(CancellationToken ct);
}
