using System.Diagnostics;
using ZenPlunger.Core.Launching;

namespace ZenPlunger.Platform.Windows.Launching;

public sealed class SteamPinballFxLauncher : IPinballFxLauncher
{
    public const string PinballFxSteamAppId = "2328760";

    private readonly string _steamExecutablePath;

    public SteamPinballFxLauncher(string steamExecutablePath = "steam.exe")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(steamExecutablePath);

        _steamExecutablePath = steamExecutablePath;
    }

    public Task LaunchAsync(LaunchRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        using var process = Process.Start(CreateStartInfo(request));

        if (process is null)
        {
            throw new InvalidOperationException("Steam did not start a process for the Pinball FX launch request.");
        }

        return Task.CompletedTask;
    }

    public ProcessStartInfo CreateStartInfo(LaunchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Table.Id);

        var startInfo = new ProcessStartInfo
        {
            FileName = _steamExecutablePath,
            UseShellExecute = false
        };

        startInfo.ArgumentList.Add("-applaunch");
        startInfo.ArgumentList.Add(PinballFxSteamAppId);
        startInfo.ArgumentList.Add("-Table");
        startInfo.ArgumentList.Add(request.Table.Id);

        if (!string.IsNullOrWhiteSpace(request.GameMode))
        {
            startInfo.ArgumentList.Add("-GameMode");
            startInfo.ArgumentList.Add(request.GameMode);
        }

        return startInfo;
    }
}

