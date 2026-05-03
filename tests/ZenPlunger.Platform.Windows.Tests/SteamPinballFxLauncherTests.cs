using ZenPlunger.Core.Launching;
using ZenPlunger.Core.Tables;
using ZenPlunger.Platform.Windows.Launching;

namespace ZenPlunger.Platform.Windows.Tests;

public sealed class SteamPinballFxLauncherTests
{
    [Fact]
    public void CreateStartInfo_BuildsSteamLaunchCommandForTable()
    {
        var launcher = new SteamPinballFxLauncher(@"C:\Steam\steam.exe");
        var request = new LaunchRequest(new PinballTable("Table_201", "Diner"));

        var startInfo = launcher.CreateStartInfo(request);

        Assert.Equal(@"C:\Steam\steam.exe", startInfo.FileName);
        Assert.False(startInfo.UseShellExecute);
        Assert.Equal(
            ["-applaunch", SteamPinballFxLauncher.PinballFxSteamAppId, "-Table", "Table_201"],
            startInfo.ArgumentList);
    }

    [Theory]
    [InlineData("Classic")]
    [InlineData("Hotseat2")]
    public void CreateStartInfo_IncludesGameModeWhenProvided(string gameMode)
    {
        var launcher = new SteamPinballFxLauncher();
        var request = new LaunchRequest(new PinballTable("Table_119", "Attack from Mars"), gameMode);

        var startInfo = launcher.CreateStartInfo(request);

        Assert.Equal(
            ["-applaunch", SteamPinballFxLauncher.PinballFxSteamAppId, "-Table", "Table_119", "-GameMode", gameMode],
            startInfo.ArgumentList);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateStartInfo_OmitsGameModeWhenBlank(string? gameMode)
    {
        var launcher = new SteamPinballFxLauncher();
        var request = new LaunchRequest(new PinballTable("Table_33", "Sorcerer's Lair"), gameMode);

        var startInfo = launcher.CreateStartInfo(request);

        Assert.Equal(
            ["-applaunch", SteamPinballFxLauncher.PinballFxSteamAppId, "-Table", "Table_33"],
            startInfo.ArgumentList);
    }
}
