namespace ZenPlunger.Core.Tables;

public sealed record TableMetadata(
    string? SourceGameId = null,
    string? SourceTableId = null,
    string? EmulatorId = null,
    string? GameFileName = null,
    string? Manufacturer = null,
    int? Year = null,
    string? Rom = null,
    string? Category = null,
    string? Theme = null,
    string? GameType = null,
    string? Author = null,
    string? DesignedBy = null,
    string? LaunchCustomVar = null,
    string? GameVersion = null,
    string? DisplayProfile = null,
    IReadOnlyList<string>? GameModes = null,
    IReadOnlyList<string>? Features = null,
    string? GameDirectory = null,
    string? MediaDirectory = null,
    IReadOnlyDictionary<string, string>? AdditionalSourceFields = null,
    bool IsVisible = true);
