using System.Windows;
using System.Windows.Interop;
using ZenPlunger.Core.Launching;
using ZenPlunger.Core.Runtime;
using ZenPlunger.Core.Tables;
using ZenPlunger.Platform.Windows.Runtime;

namespace ZenPlunger.App;

public partial class MainWindow : Window
{
    private readonly IPinballFxLauncher _launcher;
    private readonly IOverlayController _overlayController;
    private readonly IPinballFxProcessMonitor _processMonitor;
    private readonly ITableCatalog _tableCatalog;
    private GlobalHotKeyRegistration? _overlayHotKeyRegistration;

    public MainWindow(
        IPinballFxLauncher launcher,
        IPinballFxProcessMonitor processMonitor,
        ITableCatalog tableCatalog,
        IOverlayController overlayController)
    {
        ArgumentNullException.ThrowIfNull(launcher);
        ArgumentNullException.ThrowIfNull(processMonitor);
        ArgumentNullException.ThrowIfNull(tableCatalog);
        ArgumentNullException.ThrowIfNull(overlayController);

        _launcher = launcher;
        _overlayController = overlayController;
        _processMonitor = processMonitor;
        _tableCatalog = tableCatalog;

        InitializeComponent();

        Loaded += MainWindow_Loaded;
        SourceInitialized += MainWindow_SourceInitialized;
        Closed += MainWindow_Closed;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var tables = await _tableCatalog.GetTablesAsync();

        TableList.ItemsSource = tables;
        TableList.SelectedIndex = tables.Count > 0 ? 0 : -1;
        StatusText.Text = tables.Count > 0
            ? $"Loaded {tables.Count} tables from the JSON catalog."
            : "No tables found in the JSON catalog.";

        await RefreshProcessStateAsync();
    }

    private async void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        if (TableList.SelectedItem is not PinballTable table)
        {
            StatusText.Text = "Select a table first.";
            return;
        }

        try
        {
            StatusText.Text = $"Launching {table.DisplayName}...";
            await _launcher.LaunchAsync(new LaunchRequest(table));
            StatusText.Text = $"Launch request sent for {table.DisplayName}.";
            await RefreshProcessStateAsync();
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
        }
    }

    private async void RefreshStatusButton_Click(object sender, RoutedEventArgs e) =>
        await RefreshProcessStateAsync();

    private async void ShowOverlayButton_Click(object sender, RoutedEventArgs e) =>
        await _overlayController.ShowAsync();

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        var windowHandle = new WindowInteropHelper(this).Handle;
        var source = HwndSource.FromHwnd(windowHandle);

        if (source is null)
        {
            return;
        }

        _overlayHotKeyRegistration = new GlobalHotKeyRegistration(
            windowHandle,
            HotKeyModifiers.Control | HotKeyModifiers.Alt,
            virtualKey: 0x20);

        source.AddHook(WndProc);
    }

    private void MainWindow_Closed(object? sender, EventArgs e) =>
        _overlayHotKeyRegistration?.Dispose();

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (_overlayHotKeyRegistration?.MatchesMessage(msg, wParam) == true)
        {
            handled = true;
            _ = Dispatcher.InvokeAsync(async () => await _overlayController.ToggleAsync());
        }

        return nint.Zero;
    }

    private async Task RefreshProcessStateAsync()
    {
        var status = await _processMonitor.GetStatusAsync();

        ProcessStateText.Text = status.IsRunning
            ? $"Pinball FX is running ({status.ProcessCount} process{(status.ProcessCount == 1 ? string.Empty : "es")})."
            : "Pinball FX is not running.";
    }
}
