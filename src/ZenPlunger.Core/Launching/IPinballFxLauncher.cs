using ZenPlunger.Core.Tables;

namespace ZenPlunger.Core.Launching;

public interface IPinballFxLauncher
{
    Task LaunchAsync(LaunchRequest request, CancellationToken cancellationToken = default);
}

