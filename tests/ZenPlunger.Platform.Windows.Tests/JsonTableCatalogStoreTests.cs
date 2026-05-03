using ZenPlunger.Platform.Windows.Data;

namespace ZenPlunger.Platform.Windows.Tests;

public sealed class JsonTableCatalogStoreTests
{
    [Fact]
    public async Task ImportAsync_PreservesSourceTableMetadata()
    {
        var tempDirectory = Directory.CreateTempSubdirectory();

        try
        {
            var sourcePath = Path.Combine(tempDirectory.FullName, "pinup-export.json");
            var catalogPath = Path.Combine(tempDirectory.FullName, "catalog.json");

            await File.WriteAllTextAsync(sourcePath, """
                {
                  "GameExport": [
                    {
                      "GameName": "Table_109",
                      "GameDisplay": "Medieval Madness",
                      "GameID": "515",
                      "TableID": "109",
                      "Features": "Pro; Classic; SSF",
                      "Manufact": "Williams",
                      "AltRunMode": "Hotseat2,Classic",
                      "Visible": "1",
                      "CustomField": "keep me"
                    }
                  ]
                }
                """);

            var store = new JsonTableCatalogStore(catalogPath);

            var result = await store.ImportAsync(sourcePath);
            var document = await store.LoadDocumentAsync();
            var table = Assert.Single(document.Tables);
            var metadata = Assert.IsType<ZenPlunger.Core.Tables.TableMetadata>(table.Metadata);

            Assert.Equal(1, result.ImportedCount);
            Assert.Equal("Table_109", table.Id);
            Assert.Equal("109", metadata.SourceTableId);
            Assert.Equal(["Pro", "Classic", "SSF"], metadata.Features);
            Assert.Equal("Table_109", metadata.SourceFields!["GameName"]);
            Assert.Equal("109", metadata.SourceFields["TableID"]);
            Assert.Equal("Pro; Classic; SSF", metadata.SourceFields["Features"]);
            Assert.Equal("keep me", metadata.SourceFields["CustomField"]);
        }
        finally
        {
            tempDirectory.Delete(recursive: true);
        }
    }
}
