using System.Diagnostics;
using ZenPlunger.Platform.Windows.Runtime;

namespace ZenPlunger.Platform.Windows.Tests;

public sealed class WindowsPinballFxProcessMonitorTests
{
    [Fact]
    public async Task GetStatusAsync_ReturnsNotRunningWhenNoProcessesMatch()
    {
        var monitor = new WindowsPinballFxProcessMonitor(
            processNames: ["Pinball FX", "PinballFX"],
            getProcessesByName: _ => []);

        var status = await monitor.GetStatusAsync();

        Assert.False(status.IsRunning);
        Assert.Equal(0, status.ProcessCount);
        Assert.Empty(status.MatchedProcessNames);
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsRunningStatusForMatchedProcesses()
    {
        var monitor = new WindowsPinballFxProcessMonitor(
            processNames: ["Pinball FX", "PinballFX"],
            getProcessesByName: processName => processName switch
            {
                "Pinball FX" => [new Process(), new Process()],
                "PinballFX" => [new Process()],
                _ => []
            });

        var status = await monitor.GetStatusAsync();

        Assert.True(status.IsRunning);
        Assert.Equal(3, status.ProcessCount);
        Assert.Equal(["Pinball FX", "PinballFX"], status.MatchedProcessNames);
    }
}
