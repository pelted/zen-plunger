using System.Text.Json;
using System.Text.Json.Serialization;
using ZenPlunger.Core.Tables;

namespace ZenPlunger.Platform.Windows.Data;

public sealed class JsonTableCatalogStore : ITableCatalogStore, ITableCatalogImporter
{
    private static readonly HashSet<string> KnownPinupPopperFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "AltRunMode",
        "Author",
        "Category",
        "CUSTOM2",
        "DesignedBy",
        "DirGames",
        "DirMedia",
        "EMUID",
        "EMUID_1",
        "Features",
        "GAMEVER",
        "GameDisplay",
        "GameFileName",
        "GameID",
        "GameName",
        "GameTheme",
        "GameType",
        "GameYear",
        "LaunchCustomVar",
        "Manufact",
        "Notes",
        "ROM",
        "TableID",
        "TableId",
        "Visible"
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    private readonly string _catalogPath;

    public JsonTableCatalogStore(string catalogPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogPath);

        _catalogPath = catalogPath;
    }

    public async Task<IReadOnlyList<PinballTable>> GetTablesAsync(CancellationToken cancellationToken = default)
    {
        var document = await LoadDocumentAsync(cancellationToken);

        return document.Tables;
    }

    public async Task<TableCatalogDocument> LoadDocumentAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_catalogPath))
        {
            return TableCatalogDocument.Empty;
        }

        await using var stream = File.OpenRead(_catalogPath);
        var document = await JsonSerializer.DeserializeAsync<TableCatalogDocument>(stream, JsonOptions, cancellationToken);

        return document ?? TableCatalogDocument.Empty;
    }

    public async Task SaveDocumentAsync(TableCatalogDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        var catalogDirectory = Path.GetDirectoryName(_catalogPath);

        if (!string.IsNullOrWhiteSpace(catalogDirectory))
        {
            Directory.CreateDirectory(catalogDirectory);
        }

        await using var stream = File.Create(_catalogPath);
        await JsonSerializer.SerializeAsync(stream, document, JsonOptions, cancellationToken);
    }

    public async Task<TableCatalogImportResult> ImportAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Table catalog import file was not found.", sourcePath);
        }

        await using var stream = File.OpenRead(sourcePath);
        using var jsonDocument = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var importedDocument = ReadCatalogDocument(jsonDocument.RootElement);

        if (importedDocument is null)
        {
            throw new InvalidDataException("Table catalog import file did not contain a valid catalog document.");
        }

        var warnings = Validate(importedDocument);
        await SaveDocumentAsync(importedDocument, cancellationToken);

        return new TableCatalogImportResult(importedDocument.Tables.Count, warnings);
    }

    private static TableCatalogDocument? ReadCatalogDocument(JsonElement root)
    {
        if (root.TryGetProperty("schemaVersion", out _))
        {
            return root.Deserialize<TableCatalogDocument>(JsonOptions);
        }

        if (root.TryGetProperty("GameExport", out var gameExport) && gameExport.ValueKind == JsonValueKind.Array)
        {
            return ReadPupGamesExport(gameExport);
        }

        return null;
    }

    private static TableCatalogDocument ReadPupGamesExport(JsonElement gameExport)
    {
        var tables = new List<PinballTable>();

        foreach (var game in gameExport.EnumerateArray())
        {
            var id = FirstValue(game, "GameName", "GameFileName", "ROM");
            var displayName = FirstValue(game, "GameDisplay", "GameName");

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(displayName))
            {
                continue;
            }

            tables.Add(new PinballTable(
                Id: Path.GetFileNameWithoutExtension(id),
                DisplayName: displayName.Trim(),
                Collection: EmptyToNull(GetString(game, "Manufact")),
                Metadata: new TableMetadata(
                    SourceGameId: EmptyToNull(GetString(game, "GameID")),
                    SourceTableId: EmptyToNull(FirstValue(game, "TableID", "TableId")),
                    EmulatorId: EmptyToNull(FirstValue(game, "EMUID", "EMUID_1")),
                    GameFileName: EmptyToNull(GetString(game, "GameFileName")),
                    Manufacturer: EmptyToNull(GetString(game, "Manufact")),
                    Year: ParseNullableInt(GetString(game, "GameYear")),
                    Rom: EmptyToNull(GetString(game, "ROM")),
                    Category: EmptyToNull(GetString(game, "Category")),
                    Theme: EmptyToNull(GetString(game, "GameTheme")),
                    GameType: EmptyToNull(GetString(game, "GameType")),
                    Author: EmptyToNull(GetString(game, "Author")),
                    DesignedBy: EmptyToNull(GetString(game, "DesignedBy")),
                    LaunchCustomVar: EmptyToNull(GetString(game, "LaunchCustomVar")),
                    GameVersion: EmptyToNull(GetString(game, "GAMEVER")),
                    DisplayProfile: EmptyToNull(GetString(game, "CUSTOM2")),
                    GameModes: SplitList(GetString(game, "AltRunMode")),
                    Features: SplitList(GetString(game, "Features"), ',', ';', '|'),
                    GameDirectory: EmptyToNull(GetString(game, "DirGames")),
                    MediaDirectory: EmptyToNull(GetString(game, "DirMedia")),
                    AdditionalSourceFields: CaptureAdditionalSourceFields(game),
                    IsVisible: GetString(game, "Visible") != "0"),
                Notes: EmptyToNull(GetString(game, "Notes"))));
        }

        return new TableCatalogDocument(TableCatalogDocument.CurrentSchemaVersion, tables);
    }

    private static string? FirstValue(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var value = GetString(element, propertyName);

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            _ => null
        };
    }

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? ParseNullableInt(string? value) =>
        int.TryParse(value, out var parsed) ? parsed : null;

    private static IReadOnlyList<string>? SplitList(string? value, params char[] separators)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var effectiveSeparators = separators.Length == 0 ? [','] : separators;
        var values = value
            .Split(effectiveSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();

        return values.Length == 0 ? null : values;
    }

    private static IReadOnlyDictionary<string, string>? CaptureAdditionalSourceFields(JsonElement game)
    {
        Dictionary<string, string>? additionalFields = null;

        foreach (var property in game.EnumerateObject())
        {
            if (KnownPinupPopperFields.Contains(property.Name))
            {
                continue;
            }

            var value = GetSourceFieldValue(property.Value);

            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            additionalFields ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            additionalFields[property.Name] = value.Trim();
        }

        return additionalFields;
    }

    private static string? GetSourceFieldValue(JsonElement value) =>
        value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            JsonValueKind.Array => value.GetRawText(),
            JsonValueKind.Object => value.GetRawText(),
            _ => null
        };

    private static IReadOnlyList<string> Validate(TableCatalogDocument document)
    {
        var warnings = new List<string>();

        if (document.SchemaVersion != TableCatalogDocument.CurrentSchemaVersion)
        {
            warnings.Add($"Catalog schema version {document.SchemaVersion} is not the current version {TableCatalogDocument.CurrentSchemaVersion}.");
        }

        foreach (var table in document.Tables)
        {
            if (string.IsNullOrWhiteSpace(table.Id))
            {
                warnings.Add($"Table '{table.DisplayName}' is missing an ID.");
            }

            if (string.IsNullOrWhiteSpace(table.DisplayName))
            {
                warnings.Add($"Table '{table.Id}' is missing a display name.");
            }

            ValidateAsset(table.Id, "backglass", table.Assets?.Backglass, warnings);
            ValidateAsset(table.Id, "DMD", table.Assets?.Dmd, warnings);
        }

        return warnings;
    }

    private static void ValidateAsset(
        string tableId,
        string assetName,
        TableVisualAsset? asset,
        ICollection<string> warnings)
    {
        if (asset is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(asset.Path))
        {
            warnings.Add($"Table '{tableId}' has a {assetName} asset without a path.");
        }

        if (asset.Placement is { Width: <= 0 } or { Height: <= 0 })
        {
            warnings.Add($"Table '{tableId}' has a {assetName} placement with a non-positive size.");
        }
    }
}
