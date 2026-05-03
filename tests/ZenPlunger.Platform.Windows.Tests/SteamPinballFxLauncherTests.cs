using ZenPlunger.Core.Launching;
using ZenPlunger.Core.Tables;
using ZenPlunger.Platform.Windows.Configuration;
using ZenPlunger.Platform.Windows.Launching;

namespace ZenPlunger.Platform.Windows.Tests;

public sealed class SteamPinballFxLauncherTests
{
    [Fact]
    public void CreateStartInfo_BuildsSteamLaunchCommandForTable()
    {
        var launcher = new SteamPinballFxLauncher(@"C:\Steam\steam.exe");
        var table = new PinballTable(
            "Table_201",
            "Diner",
            Metadata: new TableMetadata(SourceTableId: "201"));
        var request = new LaunchRequest(table);

        var startInfo = launcher.CreateStartInfo(request);

        Assert.Equal(@"C:\Steam\steam.exe", startInfo.FileName);
        Assert.True(startInfo.UseShellExecute);
        Assert.Equal(@"C:\Steam", startInfo.WorkingDirectory);
        Assert.Equal(
            ["-applaunch", SteamPinballFxLauncher.PinballFxSteamAppId, "-Table", "201", "-GameMode", LaunchRequest.DefaultGameMode],
            startInfo.ArgumentList);
    }

    [Theory]
    [InlineData("Classic")]
    [InlineData("Hotseat2")]
    public void CreateStartInfo_IncludesGameModeWhenProvided(string gameMode)
    {
        var launcher = new SteamPinballFxLauncher();
        var table = new PinballTable(
            "Table_119",
            "Attack from Mars",
            Metadata: new TableMetadata(SourceTableId: "119"));
        var request = new LaunchRequest(table, gameMode);

        var startInfo = launcher.CreateStartInfo(request);

        Assert.Equal(
            ["-applaunch", SteamPinballFxLauncher.PinballFxSteamAppId, "-Table", "119", "-GameMode", gameMode],
            startInfo.ArgumentList);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateStartInfo_OmitsGameModeWhenBlank(string? gameMode)
    {
        var launcher = new SteamPinballFxLauncher();
        var table = new PinballTable(
            "Table_33",
            "Sorcerer's Lair",
            Metadata: new TableMetadata(SourceTableId: "33"));
        var request = new LaunchRequest(table, gameMode);

        var startInfo = launcher.CreateStartInfo(request);

        Assert.Equal(
            ["-applaunch", SteamPinballFxLauncher.PinballFxSteamAppId, "-Table", "33"],
            startInfo.ArgumentList);
    }

    [Fact]
    public void CreateStartInfo_DoesNotSetWorkingDirectoryForRelativeSteamPath()
    {
        var launcher = new SteamPinballFxLauncher("steam.exe");
        var table = new PinballTable(
            "Table_201",
            "Diner",
            Metadata: new TableMetadata(SourceTableId: "201"));
        var request = new LaunchRequest(table);

        var startInfo = launcher.CreateStartInfo(request);

        Assert.True(string.IsNullOrWhiteSpace(startInfo.WorkingDirectory));
        Assert.Equal(
            ["-applaunch", SteamPinballFxLauncher.PinballFxSteamAppId, "-Table", "201", "-GameMode", LaunchRequest.DefaultGameMode],
            startInfo.ArgumentList);
    }

    [Fact]
    public void CreateStartInfo_UsesConfiguredSteamFolderWhenProvided()
    {
        var settings = new SteamLaunchSettings
        {
            SteamFolderPath = @"C:\Program Files (x86)\Steam"
        };
        var launcher = new SteamPinballFxLauncher(settings);
        var request = new LaunchRequest(new PinballTable("Table_201", "Diner"));

        var startInfo = launcher.CreateStartInfo(request);

        Assert.Equal(@"C:\Program Files (x86)\Steam\steam.exe", startInfo.FileName);
        Assert.Equal(@"C:\Program Files (x86)\Steam", startInfo.WorkingDirectory);
    }

    [Fact]
    public void CreateStartInfo_FallsBackToStableTableIdWhenSourceTableIdIsMissing()
    {
        var launcher = new SteamPinballFxLauncher();
        var request = new LaunchRequest(new PinballTable("Table_201", "Diner"));

        var startInfo = launcher.CreateStartInfo(request);

        Assert.Equal(
            ["-applaunch", SteamPinballFxLauncher.PinballFxSteamAppId, "-Table", "Table_201", "-GameMode", LaunchRequest.DefaultGameMode],
            startInfo.ArgumentList);
    }
}
