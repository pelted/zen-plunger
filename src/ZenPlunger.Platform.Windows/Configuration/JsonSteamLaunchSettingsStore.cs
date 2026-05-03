using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZenPlunger.Platform.Windows.Configuration;

public sealed class JsonSteamLaunchSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    private readonly string _settingsPath;

    public JsonSteamLaunchSettingsStore(string settingsPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(settingsPath);

        _settingsPath = settingsPath;
    }

    public async Task<SteamLaunchSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_settingsPath))
        {
            return new SteamLaunchSettings();
        }

        await using var stream = File.OpenRead(_settingsPath);
        var settings = await JsonSerializer.DeserializeAsync<SteamLaunchSettings>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return settings ?? new SteamLaunchSettings();
    }

    public async Task SaveAsync(SteamLaunchSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var directory = Path.GetDirectoryName(_settingsPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_settingsPath);
        await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }
}
