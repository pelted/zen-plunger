using System.Diagnostics;
using System.Runtime.InteropServices;
using ZenPlunger.Core.Runtime;

namespace ZenPlunger.Platform.Windows.Runtime;

public sealed class WindowsPinballFxTableSessionController : IPinballFxTableSessionController
{
    private static readonly string[] DefaultProcessNames = ["Pinball FX", "PinballFX"];
    private static readonly ushort[] ExitTableSequence = [0x26, 0x0D, 0x25, 0x0D];
    private const uint InputTypeKeyboard = 1;
    private const uint KeyEventExtendedKey = 0x0001;
    private const uint KeyEventKeyUp = 0x0002;
    private const uint KeyEventScanCode = 0x0008;

    private readonly IReadOnlyList<string> _processNames;
    private readonly Func<string, Process[]> _getProcessesByName;
    private readonly Func<nint> _getMainWindowHandle;
    private readonly Action<nint> _restoreWindow;
    private readonly Func<nint, bool> _setForegroundWindow;
    private readonly Func<nint> _getForegroundWindow;
    private readonly Func<nint, ushort, bool> _sendVirtualKeyToWindow;
    private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync;

    public WindowsPinballFxTableSessionController(
        IEnumerable<string>? processNames = null,
        Func<string, Process[]>? getProcessesByName = null,
        Func<nint>? getMainWindowHandle = null,
        Action<nint>? restoreWindow = null,
        Func<nint, bool>? setForegroundWindow = null,
        Func<nint>? getForegroundWindow = null,
        Func<nint, ushort, bool>? sendVirtualKeyToWindow = null,
        Func<TimeSpan, CancellationToken, Task>? delayAsync = null)
    {
        _processNames = (processNames ?? DefaultProcessNames)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (_processNames.Count == 0)
        {
            throw new ArgumentException("At least one Pinball FX process name must be provided.", nameof(processNames));
        }

        _getProcessesByName = getProcessesByName ?? Process.GetProcessesByName;
        _getMainWindowHandle = getMainWindowHandle ?? FindMainWindowHandle;
        _restoreWindow = restoreWindow ?? RestoreWindow;
        _setForegroundWindow = setForegroundWindow ?? SetForegroundWindow;
        _getForegroundWindow = getForegroundWindow ?? GetForegroundWindow;
        _sendVirtualKeyToWindow = sendVirtualKeyToWindow ?? SendVirtualKeyToForegroundWindow;
        _delayAsync = delayAsync ?? Task.Delay;
    }

    public async Task ExitCurrentTableToMenuAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var windowHandle = _getMainWindowHandle();

        if (windowHandle == nint.Zero)
        {
            throw new InvalidOperationException("Pinball FX is not running with a visible window.");
        }

        _restoreWindow(windowHandle);
        _setForegroundWindow(windowHandle);
        await _delayAsync(TimeSpan.FromMilliseconds(500), cancellationToken);

        if (_getForegroundWindow() != windowHandle)
        {
            throw new InvalidOperationException("Failed to return focus to the Pinball FX window.");
        }

        foreach (var virtualKey in ExitTableSequence)
        {
            if (!_sendVirtualKeyToWindow(windowHandle, virtualKey))
            {
                throw new InvalidOperationException($"Failed to send virtual key 0x{virtualKey:X2} to the Pinball FX window.");
            }

            await _delayAsync(TimeSpan.FromMilliseconds(250), cancellationToken);
        }

        await _delayAsync(TimeSpan.FromMilliseconds(1500), cancellationToken);
    }

    private nint FindMainWindowHandle()
    {
        foreach (var processName in _processNames)
        {
            var processes = _getProcessesByName(processName);

            try
            {
                var targetProcess = processes.FirstOrDefault(process => process.MainWindowHandle != nint.Zero);

                if (targetProcess is not null)
                {
                    return targetProcess.MainWindowHandle;
                }
            }
            finally
            {
                foreach (var process in processes)
                {
                    process.Dispose();
                }
            }
        }

        return nint.Zero;
    }

    private static void RestoreWindow(nint windowHandle) =>
        ShowWindow(windowHandle, 9);

    private static bool SendVirtualKeyToForegroundWindow(nint _, ushort virtualKey)
    {
        var scanCode = (ushort)MapVirtualKey(virtualKey, 0);
        var keyFlags = KeyEventScanCode | GetExtendedKeyFlag(virtualKey);
        INPUT[] inputs =
        [
            new()
            {
                type = InputTypeKeyboard,
                Anonymous = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wScan = scanCode,
                        dwFlags = keyFlags
                    }
                }
            },
            new()
            {
                type = InputTypeKeyboard,
                Anonymous = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wScan = scanCode,
                        dwFlags = keyFlags | KeyEventKeyUp
                    }
                }
            }
        ];

        var sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>()) == inputs.Length;
        Thread.Sleep(50);
        return sent;
    }

    private static uint GetExtendedKeyFlag(ushort virtualKey) =>
        virtualKey is 0x25 or 0x26 ? KeyEventExtendedKey : 0;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(nint hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public INPUTUNION Anonymous;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUTUNION
    {
        [FieldOffset(0)]
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public nuint dwExtraInfo;
    }
}
