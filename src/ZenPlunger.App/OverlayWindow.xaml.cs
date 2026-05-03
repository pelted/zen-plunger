using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ZenPlunger.Core.Launching;
using ZenPlunger.Core.Runtime;
using ZenPlunger.Core.Tables;

namespace ZenPlunger.App;

public partial class OverlayWindow : Window
{
    private readonly IPinballFxLauncher _launcher;
    private readonly IPinballFxProcessMonitor _processMonitor;
    private readonly ITableCatalog _tableCatalog;
    private bool _allowClose;
    private bool _tablesLoaded;

    public OverlayWindow(
        IPinballFxLauncher launcher,
        IPinballFxProcessMonitor processMonitor,
        ITableCatalog tableCatalog)
    {
        ArgumentNullException.ThrowIfNull(launcher);
        ArgumentNullException.ThrowIfNull(processMonitor);
        ArgumentNullException.ThrowIfNull(tableCatalog);

        _launcher = launcher;
        _processMonitor = processMonitor;
        _tableCatalog = tableCatalog;

        InitializeComponent();

        Loaded += OverlayWindow_Loaded;
        Closing += OverlayWindow_Closing;
        PreviewKeyDown += OverlayWindow_PreviewKeyDown;
    }

    public void PrepareForClose() => _allowClose = true;

    private async void OverlayWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await EnsureTablesLoadedAsync();
        await RefreshProcessStateAsync();
    }

    private void OverlayWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (_allowClose)
        {
            return;
        }

        e.Cancel = true;
        Hide();
    }

    private void OverlayWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Hide();
    }

    private async void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        if (OverlayTableList.SelectedItem is not PinballTable table)
        {
            OverlayStatusText.Text = "Select a table first.";
            return;
        }

        try
        {
            var processStatus = await _processMonitor.GetStatusAsync();
            var launchMode = processStatus.IsRunning ? "Warm" : "Cold";

            OverlayStatusText.Text = $"{launchMode} launch request for {table.DisplayName}...";
            await _launcher.LaunchAsync(new LaunchRequest(table));
            OverlayStatusText.Text = $"{launchMode} launch request sent for {table.DisplayName}.";
            await RefreshProcessStateAsync();
            Hide();
        }
        catch (Exception ex)
        {
            OverlayStatusText.Text = ex.Message;
        }
    }

    private async void RefreshStatusButton_Click(object sender, RoutedEventArgs e) =>
        await RefreshProcessStateAsync();

    private void HideOverlayButton_Click(object sender, RoutedEventArgs e) =>
        Hide();

    private async Task EnsureTablesLoadedAsync()
    {
        if (_tablesLoaded)
        {
            return;
        }

        var tables = await _tableCatalog.GetTablesAsync();

        OverlayTableList.ItemsSource = tables;
        OverlayTableList.SelectedIndex = tables.Count > 0 ? 0 : -1;
        _tablesLoaded = true;
    }

    private async Task RefreshProcessStateAsync()
    {
        var status = await _processMonitor.GetStatusAsync();

        OverlayProcessStateText.Text = status.IsRunning
            ? $"Pinball FX is running ({status.ProcessCount} process{(status.ProcessCount == 1 ? string.Empty : "es")})."
            : "Pinball FX is not running.";
    }
}
