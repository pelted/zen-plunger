using System.IO;
using System.Windows;
using ZenPlunger.Core.Launching;
using ZenPlunger.Core.Tables;
using ZenPlunger.Platform.Windows.Data;
using ZenPlunger.Platform.Windows.Launching;

namespace ZenPlunger.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = new MainWindow(
            CreateLauncher(),
            CreateTableCatalog());

        MainWindow = mainWindow;
        mainWindow.Show();
    }

    private static IPinballFxLauncher CreateLauncher() =>
        new SteamPinballFxLauncher();

    private static ITableCatalog CreateTableCatalog() =>
        new JsonTableCatalogStore(
            Path.Combine(AppContext.BaseDirectory, "data", "tables.sample.json"));
}
