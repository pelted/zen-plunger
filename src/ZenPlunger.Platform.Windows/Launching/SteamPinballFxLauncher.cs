using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using ZenPlunger.Core.Launching;
using ZenPlunger.Platform.Windows.Configuration;

namespace ZenPlunger.Platform.Windows.Launching;

public sealed class SteamPinballFxLauncher : IPinballFxLauncher
{
    public const string PinballFxSteamAppId = "2328760";

    private readonly string _fallbackSteamExecutablePath;
    private readonly SteamLaunchSettings? _settings;

    public SteamPinballFxLauncher(string steamExecutablePath = "steam.exe")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(steamExecutablePath);

        _fallbackSteamExecutablePath = steamExecutablePath;
    }

    public SteamPinballFxLauncher(SteamLaunchSettings settings, string fallbackSteamExecutablePath = "steam.exe")
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(fallbackSteamExecutablePath);

        _settings = settings;
        _fallbackSteamExecutablePath = fallbackSteamExecutablePath;
    }

    public Task LaunchAsync(LaunchRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using var process = Process.Start(CreateStartInfo(request));

            if (process is null)
            {
                throw new InvalidOperationException("Steam did not start a process for the Pinball FX launch request.");
            }
        }
        catch (Win32Exception ex)
        {
            var steamExecutablePath = ResolveSteamExecutablePath();
            throw new InvalidOperationException(
                $"Failed to start Steam using '{steamExecutablePath}'. {ex.Message}",
                ex);
        }

        return Task.CompletedTask;
    }

    public ProcessStartInfo CreateStartInfo(LaunchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var launchTableId = ResolveLaunchTableId(request.Table);

        var steamExecutablePath = ResolveSteamExecutablePath();

        var startInfo = new ProcessStartInfo
        {
            FileName = steamExecutablePath,
            UseShellExecute = true
        };

        if (Path.IsPathRooted(steamExecutablePath))
        {
            var workingDirectory = Path.GetDirectoryName(steamExecutablePath);

            if (!string.IsNullOrWhiteSpace(workingDirectory))
            {
                startInfo.WorkingDirectory = workingDirectory;
            }
        }

        startInfo.ArgumentList.Add("-applaunch");
        startInfo.ArgumentList.Add(PinballFxSteamAppId);
        startInfo.ArgumentList.Add("-Table");
        startInfo.ArgumentList.Add(launchTableId);

        if (!string.IsNullOrWhiteSpace(request.GameMode))
        {
            startInfo.ArgumentList.Add("-GameMode");
            startInfo.ArgumentList.Add(request.GameMode);
        }

        return startInfo;
    }

    private string ResolveSteamExecutablePath()
    {
        if (!string.IsNullOrWhiteSpace(_settings?.SteamFolderPath))
        {
            return Path.Combine(_settings.SteamFolderPath.Trim(), "steam.exe");
        }

        return _fallbackSteamExecutablePath;
    }

    private static string ResolveLaunchTableId(Core.Tables.PinballTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        if (!string.IsNullOrWhiteSpace(table.Metadata?.SourceTableId))
        {
            return table.Metadata.SourceTableId;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(table.Id);
        return table.Id;
    }
}
