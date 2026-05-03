using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ZenPlunger.Platform.Windows.Runtime;

[Flags]
public enum HotKeyModifiers : uint
{
    None = 0,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Windows = 0x0008
}

public sealed class GlobalHotKeyRegistration : IDisposable
{
    public const int WindowMessageHotKey = 0x0312;

    private static int _nextId;

    private readonly nint _windowHandle;
    private bool _disposed;

    public GlobalHotKeyRegistration(nint windowHandle, HotKeyModifiers modifiers, uint virtualKey)
    {
        if (windowHandle == nint.Zero)
        {
            throw new ArgumentException("A valid window handle is required.", nameof(windowHandle));
        }

        _windowHandle = windowHandle;
        HotKeyId = Interlocked.Increment(ref _nextId);

        if (!RegisterHotKey(windowHandle, HotKeyId, (uint)modifiers, virtualKey))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to register the global hotkey.");
        }
    }

    public int HotKeyId { get; }

    public bool MatchesMessage(int message, nint wParam) =>
        message == WindowMessageHotKey && wParam == HotKeyId;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        UnregisterHotKey(_windowHandle, HotKeyId);
        _disposed = true;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(nint hWnd, int id);
}
