using System.Diagnostics;
using ZenPlunger.Core.Runtime;

namespace ZenPlunger.Platform.Windows.Runtime;

public sealed class WindowsPinballFxProcessMonitor : IPinballFxProcessMonitor
{
    private static readonly string[] DefaultProcessNames = ["Pinball FX", "PinballFX"];

    private readonly IReadOnlyList<string> _processNames;
    private readonly Func<string, Process[]> _getProcessesByName;

    public WindowsPinballFxProcessMonitor(
        IEnumerable<string>? processNames = null,
        Func<string, Process[]>? getProcessesByName = null)
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
    }

    public Task<PinballFxProcessStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var matchedProcessNames = new List<string>();
        var processCount = 0;

        foreach (var processName in _processNames)
        {
            var processes = _getProcessesByName(processName);

            if (processes.Length > 0)
            {
                matchedProcessNames.Add(processName);
            }

            foreach (var process in processes)
            {
                processCount++;
                process.Dispose();
            }
        }

        return Task.FromResult(new PinballFxProcessStatus(
            IsRunning: processCount > 0,
            ProcessCount: processCount,
            MatchedProcessNames: matchedProcessNames));
    }
}
