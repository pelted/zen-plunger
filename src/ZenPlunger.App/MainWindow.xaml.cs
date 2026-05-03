using System.Windows;
using ZenPlunger.Core.Launching;
using ZenPlunger.Core.Tables;
using ZenPlunger.Platform.Windows.Launching;

namespace ZenPlunger.App;

public partial class MainWindow : Window
{
    private readonly IPinballFxLauncher _launcher = new SteamPinballFxLauncher();

    public MainWindow()
    {
        InitializeComponent();

        TableList.ItemsSource = GetStarterTables();
        TableList.SelectedIndex = 0;
    }

    private static IReadOnlyList<PinballTable> GetStarterTables() =>
    [
        new("Williams_Medieval_Madness", "Medieval Madness", "Williams"),
        new("Williams_Attack_From_Mars", "Attack from Mars", "Williams"),
        new("Zen_Sorcerers_Lair", "Sorcerer's Lair", "Zen Originals")
    ];

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
