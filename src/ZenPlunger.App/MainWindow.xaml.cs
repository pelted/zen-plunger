using System.IO;
using System.Windows;
using ZenPlunger.Core.Launching;
using ZenPlunger.Core.Tables;
using ZenPlunger.Platform.Windows.Data;
using ZenPlunger.Platform.Windows.Launching;

namespace ZenPlunger.App;

public partial class MainWindow : Window
{
    private readonly IPinballFxLauncher _launcher = new SteamPinballFxLauncher();
    private readonly ITableCatalog _tableCatalog = new JsonTableCatalogStore(
        Path.Combine(AppContext.BaseDirectory, "data", "tables.sample.json"));

    public MainWindow()
    {
        InitializeComponent();

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var tables = await _tableCatalog.GetTablesAsync();

        TableList.ItemsSource = tables;
        TableList.SelectedIndex = tables.Count > 0 ? 0 : -1;
        StatusText.Text = tables.Count > 0
            ? $"Loaded {tables.Count} tables from the JSON catalog."
            : "No tables found in the JSON catalog.";
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
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
        }
    }
}
