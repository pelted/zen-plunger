using ZenPlunger.Platform.Windows.Runtime;

namespace ZenPlunger.Platform.Windows.Tests;

public sealed class WindowsPinballFxTableSessionControllerTests
{
    [Fact]
    public async Task ExitCurrentTableToMenuAsync_SendsExpectedPauseExitSequence()
    {
        var sentKeys = new List<ushort>();
        var restoreHandles = new List<nint>();
        var foregroundHandles = new List<nint>();
        var delays = new List<TimeSpan>();

        var controller = new WindowsPinballFxTableSessionController(
            getMainWindowHandle: () => (nint)1234,
            restoreWindow: handle =>
            {
                restoreHandles.Add(handle);
            },
            setForegroundWindow: handle =>
            {
                foregroundHandles.Add(handle);
                return true;
            },
            getForegroundWindow: () => (nint)1234,
            sendVirtualKeyToWindow: (handle, virtualKey) =>
            {
                Assert.Equal((nint)1234, handle);
                sentKeys.Add(virtualKey);
                return true;
            },
            delayAsync: (delay, _) =>
            {
                delays.Add(delay);
                return Task.CompletedTask;
            });

        await controller.ExitCurrentTableToMenuAsync();

        Assert.Equal([(nint)1234], restoreHandles);
        Assert.Equal([(nint)1234], foregroundHandles);
        Assert.Equal([0x26, 0x0D, 0x25, 0x0D], sentKeys);
        Assert.Equal(
            [TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(1500)],
            delays);
    }

    [Fact]
    public async Task ExitCurrentTableToMenuAsync_ThrowsWhenNoWindowHandleIsAvailable()
    {
        var controller = new WindowsPinballFxTableSessionController(
            getMainWindowHandle: () => nint.Zero);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => controller.ExitCurrentTableToMenuAsync());

        Assert.Contains("visible window", exception.Message);
    }

    [Fact]
    public async Task ExitCurrentTableToMenuAsync_ThrowsWhenKeyDeliveryFails()
    {
        var controller = new WindowsPinballFxTableSessionController(
            getMainWindowHandle: () => (nint)1234,
            restoreWindow: _ => { },
            setForegroundWindow: _ => true,
            getForegroundWindow: () => (nint)1234,
            sendVirtualKeyToWindow: (_, _) => false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => controller.ExitCurrentTableToMenuAsync());

        Assert.Contains("virtual key", exception.Message);
    }

    [Fact]
    public async Task ExitCurrentTableToMenuAsync_ThrowsWhenForegroundWindowDoesNotSwitch()
    {
        var controller = new WindowsPinballFxTableSessionController(
            getMainWindowHandle: () => (nint)1234,
            restoreWindow: _ => { },
            setForegroundWindow: _ => true,
            getForegroundWindow: () => (nint)5678,
            sendVirtualKeyToWindow: (_, _) => true);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => controller.ExitCurrentTableToMenuAsync());

        Assert.Contains("focus", exception.Message);
    }
}
