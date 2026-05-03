namespace ZenPlunger.Core.Runtime;

public interface IPinballFxProcessMonitor
{
    Task<PinballFxProcessStatus> GetStatusAsync(CancellationToken cancellationToken = default);
}
