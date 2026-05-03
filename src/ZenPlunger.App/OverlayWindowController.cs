using System.Windows;
using ZenPlunger.Core.Runtime;

namespace ZenPlunger.App;

public sealed class OverlayWindowController : IOverlayController, IDisposable
{
    private readonly Func<OverlayWindow> _overlayWindowFactory;
    private OverlayWindow? _overlayWindow;
    private bool _disposed;

    public OverlayWindowController(Func<OverlayWindow> overlayWindowFactory)
    {
        ArgumentNullException.ThrowIfNull(overlayWindowFactory);

        _overlayWindowFactory = overlayWindowFactory;
    }

    public bool IsVisible => _overlayWindow?.IsVisible == true;

    public Task ShowAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var overlayWindow = GetOrCreateOverlayWindow();

        if (!overlayWindow.IsVisible)
        {
            overlayWindow.Show();
        }

        if (overlayWindow.WindowState == WindowState.Minimized)
        {
            overlayWindow.WindowState = WindowState.Normal;
        }

        overlayWindow.Activate();
        overlayWindow.Focus();

        return Task.CompletedTask;
    }

    public Task HideAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (_overlayWindow?.IsVisible == true)
        {
            _overlayWindow.Hide();
        }

        return Task.CompletedTask;
    }

    public Task ToggleAsync(CancellationToken cancellationToken = default) =>
        IsVisible ? HideAsync(cancellationToken) : ShowAsync(cancellationToken);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_overlayWindow is not null)
        {
            _overlayWindow.PrepareForClose();
            _overlayWindow.Close();
        }

        _disposed = true;
    }

    private OverlayWindow GetOrCreateOverlayWindow()
    {
        _overlayWindow ??= _overlayWindowFactory();
        return _overlayWindow;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
