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
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = new MainWindow(
            CreateLauncher(),
            CreateProcessMonitor(),
            CreateTableCatalog());

        MainWindow = mainWindow;
        mainWindow.Show();
    }

    private static IPinballFxLauncher CreateLauncher() =>
        new SteamPinballFxLauncher();

    private static IPinballFxProcessMonitor CreateProcessMonitor() =>
        new WindowsPinballFxProcessMonitor();

    private static ITableCatalog CreateTableCatalog() =>
        new JsonTableCatalogStore(
            Path.Combine(AppContext.BaseDirectory, "data", "tables.sample.json"));
}
