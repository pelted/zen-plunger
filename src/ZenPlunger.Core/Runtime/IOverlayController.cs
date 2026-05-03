namespace ZenPlunger.Core.Runtime;

public interface IOverlayController
{
    bool IsVisible { get; }

    Task ShowAsync(CancellationToken cancellationToken = default);

    Task HideAsync(CancellationToken cancellationToken = default);

    Task ToggleAsync(CancellationToken cancellationToken = default);
}

