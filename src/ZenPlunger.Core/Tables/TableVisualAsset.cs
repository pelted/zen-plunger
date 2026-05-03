namespace ZenPlunger.Core.Tables;

public sealed record TableVisualAsset(
    string Path,
    ScreenPlacement? Placement = null);

