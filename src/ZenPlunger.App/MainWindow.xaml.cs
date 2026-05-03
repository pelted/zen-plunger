using System.Windows;
using ZenPlunger.Core.Launching;
using ZenPlunger.Core.Tables;

namespace ZenPlunger.App;

public partial class MainWindow : Window
{
    private readonly IPinballFxLauncher _launcher;
    private readonly ITableCatalog _tableCatalog;

    public MainWindow(IPinballFxLauncher launcher, ITableCatalog tableCatalog)
    {
        ArgumentNullException.ThrowIfNull(launcher);
        ArgumentNullException.ThrowIfNull(tableCatalog);

        _launcher = launcher;
        _tableCatalog = tableCatalog;

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
