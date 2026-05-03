# Table Catalog

Zen Plunger needs a datastore for Pinball FX table metadata and cabinet assets. The first backing store is JSON because it is easy to inspect, edit, import, and replace later.

The long-term shape should remain friendly to SQLite. Keep catalog documents normalized enough that each table and asset placement can become rows later.

## Current Store

The current sample catalog lives at:

```text
data/tables.sample.json
```

The WPF app copies this file to the build output and loads it on startup through `JsonTableCatalogStore`.

## JSON Shape

```json
{
  "schemaVersion": 1,
  "tables": [
    {
      "id": "Table_109",
      "displayName": "Medieval Madness",
      "collection": "Williams",
      "metadata": {
        "sourceGameId": "515",
        "sourceTableId": "109",
        "manufacturer": "Williams",
        "features": ["Pro", "SSF"],
        "gameModes": ["Hotseat2", "Classic"],
        "isVisible": true,
        "sourceFields": {
          "GameName": "Table_109",
          "TableID": "109",
          "Features": "Pro;SSF"
        }
      },
      "assets": {
        "backglass": {
          "path": "assets/backglass/medieval-madness.png",
          "placement": {
            "screenName": "Backglass",
            "x": 0,
            "y": 0,
            "width": 1920,
            "height": 1080
          }
        },
        "dmd": {
          "path": "assets/dmd/medieval-madness.png",
          "placement": {
            "screenName": "DMD",
            "x": 0,
            "y": 0,
            "width": 1280,
            "height": 320
          }
        }
      }
    }
  ]
}
```

## Fields

- `schemaVersion`: catalog schema version. Current version is `1`.
- `tables`: list of known Pinball FX tables.
- `id`: stable catalog and launch ID used for Pinball FX launching. Prefer upstream IDs such as `Table_201`.
- `displayName`: user-facing table name.
- `collection`: optional grouping, such as `Williams` or `Zen Originals`.
- `metadata.sourceGameId`: source-specific game identifier from an imported catalog.
- `metadata.sourceTableId`: source-specific table identifier when the upstream export distinguishes it from the launch ID.
- `metadata.features`: feature tags imported from upstream data, such as cabinet or media capabilities.
- `metadata.sourceFields`: full source metadata preserved during import as raw string values so upstream fields are not lost while the typed schema evolves.
- `assets.backglass.path`: path to the backglass image or media asset.
- `assets.backglass.placement`: target screen rectangle for the backglass asset.
- `assets.dmd.path`: path to the DMD image or media asset.
- `assets.dmd.placement`: target screen rectangle for the DMD asset.
- `screenName`: logical display role. Initial values are expected to be `Backglass` and `DMD`.
- `x`, `y`, `width`, `height`: placement rectangle in screen coordinates.

## Import

`JsonTableCatalogStore` can import another JSON catalog file with the same schema and save it as the active catalog. The import path currently validates basic data quality:

- Schema version mismatch
- Missing table ID
- Missing display name
- Asset entries without paths
- Asset placements with non-positive sizes

When importing a Pinup Popper `GameExport`, the importer keeps the main typed fields we care about and also preserves the full source row under `metadata.sourceFields` so useful upstream metadata is not silently lost while the schema evolves.

A future UI should expose this through a file picker and show warnings before replacing the active catalog.

## SQLite Direction

If JSON becomes limiting, the likely SQLite shape is:

```text
tables
  id
  display_name
  collection
  notes

table_assets
  table_id
  asset_kind
  path
  screen_name
  x
  y
  width
  height
```

The app should continue to use `ITableCatalog`, `ITableCatalogStore`, and `ITableCatalogImporter` so the backing store can change without rewriting UI code.
