using System.IO;
using System.Windows;
using ZenPlunger.Core.Launching;
using ZenPlunger.Core.Runtime;
using ZenPlunger.Core.Tables;
using ZenPlunger.Platform.Windows.Configuration;
using ZenPlunger.Platform.Windows.Data;
using ZenPlunger.Platform.Windows.Launching;
using ZenPlunger.Platform.Windows.Runtime;

namespace ZenPlunger.App;

public partial class App : Application
{
    private OverlayWindowController? _overlayController;
    private string? _diagnosticLogPath;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            _diagnosticLogPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ZenPlunger",
                "startup-diagnostic.log");
            WriteDiagnostic("App.OnStartup begin.");

            DispatcherUnhandledException += (_, args) =>
            {
                WriteDiagnostic($"DispatcherUnhandledException: {args.Exception}");
            };

            var steamLaunchSettingsStore = CreateSteamLaunchSettingsStore();
            WriteDiagnostic("Loading Steam settings.");
            var steamLaunchSettings = steamLaunchSettingsStore.LoadAsync().GetAwaiter().GetResult();
            WriteDiagnostic("Creating services.");
            var launcher = CreateLauncher(steamLaunchSettings);
            var processMonitor = CreateProcessMonitor();
            var tableSessionController = CreateTableSessionController();
            var tableCatalog = CreateTableCatalog();

            _overlayController = new OverlayWindowController(() =>
                new OverlayWindow(launcher, processMonitor, tableSessionController, tableCatalog));
            WriteDiagnostic("Overlay controller created.");

            var mainWindow = new MainWindow(
                launcher,
                processMonitor,
                tableCatalog,
                _overlayController,
                steamLaunchSettings,
                steamLaunchSettingsStore);
            WriteDiagnostic("MainWindow constructed.");
            mainWindow.SourceInitialized += (_, _) => WriteDiagnostic("MainWindow SourceInitialized.");
            mainWindow.Loaded += (_, _) => WriteDiagnostic("MainWindow Loaded.");
            mainWindow.ContentRendered += (_, _) => WriteDiagnostic("MainWindow ContentRendered.");
            mainWindow.Closing += (_, _) => WriteDiagnostic("MainWindow Closing.");
            mainWindow.Closed += (_, _) => WriteDiagnostic("MainWindow Closed.");

            MainWindow = mainWindow;
            WriteDiagnostic("Calling MainWindow.Show().");
            mainWindow.Show();
            WriteDiagnostic($"MainWindow.Show() returned. Visible={mainWindow.IsVisible} State={mainWindow.WindowState}.");
        }
        catch (Exception ex)
        {
            var startupErrorPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ZenPlunger",
                "startup-error.log");

            Directory.CreateDirectory(Path.GetDirectoryName(startupErrorPath)!);
            File.WriteAllText(startupErrorPath, ex.ToString());
            MessageBox.Show(
                ex.ToString(),
                "Zen Plunger Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        WriteDiagnostic($"App.OnExit code={e.ApplicationExitCode}.");
        _overlayController?.Dispose();
        base.OnExit(e);
    }

    private void WriteDiagnostic(string message)
    {
        if (string.IsNullOrWhiteSpace(_diagnosticLogPath))
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_diagnosticLogPath)!);
        File.AppendAllText(_diagnosticLogPath, $"[{DateTimeOffset.Now:O}] {message}{Environment.NewLine}");
    }

    private static IPinballFxLauncher CreateLauncher(SteamLaunchSettings steamLaunchSettings) =>
        new SteamPinballFxLauncher(steamLaunchSettings);

    private static IPinballFxProcessMonitor CreateProcessMonitor() =>
        new WindowsPinballFxProcessMonitor();

    private static IPinballFxTableSessionController CreateTableSessionController() =>
        new WindowsPinballFxTableSessionController();

    private static ITableCatalog CreateTableCatalog() =>
        new JsonTableCatalogStore(
            Path.Combine(AppContext.BaseDirectory, "data", "tables.sample.json"));

    private static JsonSteamLaunchSettingsStore CreateSteamLaunchSettingsStore() =>
        new(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZenPlunger",
            "settings.json"));
}
