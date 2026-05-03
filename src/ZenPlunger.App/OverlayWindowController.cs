using System.Windows;
using ZenPlunger.Core.Runtime;

namespace ZenPlunger.App;

public sealed class OverlayWindowController : IOverlayController, IDisposable
{
    private readonly OverlayWindow _overlayWindow;
    private bool _disposed;

    public OverlayWindowController(OverlayWindow overlayWindow)
    {
        ArgumentNullException.ThrowIfNull(overlayWindow);

        _overlayWindow = overlayWindow;
    }

    public bool IsVisible => _overlayWindow.IsVisible;

    public Task ShowAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (!_overlayWindow.IsVisible)
        {
            _overlayWindow.Show();
        }

        if (_overlayWindow.WindowState == WindowState.Minimized)
        {
            _overlayWindow.WindowState = WindowState.Normal;
        }

        _overlayWindow.Activate();
        _overlayWindow.Focus();

        return Task.CompletedTask;
    }

    public Task HideAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (_overlayWindow.IsVisible)
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

        _overlayWindow.PrepareForClose();
        _overlayWindow.Close();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
