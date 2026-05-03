using System.IO;
using System.Windows;
using ZenPlunger.Core.Launching;
using ZenPlunger.Core.Runtime;
using ZenPlunger.Core.Tables;
using ZenPlunger.Platform.Windows.Data;
using ZenPlunger.Platform.Windows.Launching;
using ZenPlunger.Platform.Windows.Runtime;

namespace ZenPlunger.App;

public partial class App : Application
{
    private OverlayWindowController? _overlayController;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var launcher = CreateLauncher();
        var processMonitor = CreateProcessMonitor();
        var tableCatalog = CreateTableCatalog();
        var overlayWindow = new OverlayWindow(launcher, processMonitor, tableCatalog);

        _overlayController = new OverlayWindowController(overlayWindow);

        var mainWindow = new MainWindow(
            launcher,
            processMonitor,
            tableCatalog,
            _overlayController);

        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _overlayController?.Dispose();
        base.OnExit(e);
    }

    private static IPinballFxLauncher CreateLauncher() =>
        new SteamPinballFxLauncher();

    private static IPinballFxProcessMonitor CreateProcessMonitor() =>
        new WindowsPinballFxProcessMonitor();

    private static ITableCatalog CreateTableCatalog() =>
        new JsonTableCatalogStore(
            Path.Combine(AppContext.BaseDirectory, "data", "tables.sample.json"));
}
