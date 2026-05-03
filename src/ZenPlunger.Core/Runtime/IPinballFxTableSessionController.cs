namespace ZenPlunger.Core.Runtime;

public interface IPinballFxTableSessionController
{
    Task ExitCurrentTableToMenuAsync(CancellationToken cancellationToken = default);
}
