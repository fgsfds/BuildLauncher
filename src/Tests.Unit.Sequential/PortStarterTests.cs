using Addons.Addons;
using Core.All.Enums;
using Core.Client.Interfaces;
using Core.Client.Providers;
using Games.Games;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Ports.Ports;
using Tests.Unit.Helpers;

namespace Tests.Unit.Sequential;

/// <summary> Verifies launch outcomes, cleanup, and executable selection. </summary>
public sealed class PortStarterTests
{
    /// <summary> Selects the platform-specific override without replacing an explicit user choice. </summary>
    /// <param name="os">The platform being tested.</param>
    /// <param name="custom">The explicit executable.</param>
    /// <param name="expected">The expected executable.</param>
    [Theory]
    [InlineData(OSEnum.Windows, null, "windows.exe")]
    [InlineData(OSEnum.Linux, null, "linux")]
    [InlineData(OSEnum.Windows, "custom.exe", "custom.exe")]
    [InlineData(OSEnum.Linux, "custom", "custom")]
    public void ResolveExecutable_UsesPlatformAndExplicitChoice(OSEnum os, string? custom, string expected)
    {
        var port = new StubPort();
        var addon = CampaignTestFactory.CreateDukeCampaign("test", GameEnum.Duke3D, null, executables: new()
        {
            [OSEnum.Windows] = new() { [PortEnum.Stub] = "windows.exe" },
            [OSEnum.Linux] = new() { [PortEnum.Stub] = "linux" }
        });
        Assert.Equal(expected, PortStarter.ResolveExecutable(port, addon, os, custom));
    }

    /// <summary> Missing overrides preserve the custom executable or use the port default. </summary>
    [Fact]
    public void ResolveExecutable_MissingPort_PreservesFallback()
    {
        var port = new StubPort();
        var addon = CampaignTestFactory.CreateDukeCampaign("test", GameEnum.Duke3D, null, executables: new()
        {
            [OSEnum.Windows] = new() { [PortEnum.EDuke32] = "other.exe" }
        });
        Assert.Equal("custom.exe", PortStarter.ResolveExecutable(port, addon, OSEnum.Windows, "custom.exe"));
        Assert.Equal(port.PortExeFilePath, PortStarter.ResolveExecutable(port, addon, OSEnum.Windows));
        Assert.Equal(port.PortExeFilePath, PortStarter.ResolveExecutable(port, addon, OSEnum.Linux));
    }

    /// <summary> Every failure is observable, cleanup is paired with setup, and process handles are disposed. </summary>
    /// <param name="failure">The failure stage, or success.</param>
    [Theory]
    [InlineData("success")]
    [InlineData("null")]
    [InlineData("start")]
    [InlineData("wait")]
    [InlineData("args")]
    [InlineData("playtime")]
    [InlineData("cleanup")]
    [InlineData("setup")]
    [InlineData("start-cleanup")]
    public async Task StartAsync_HandlesFailureAndCleanup(string failure)
    {
        var config = new Mock<IConfigProvider>();
        config.SetupGet(x => x.Playtimes).Returns(new Dictionary<string, TimeSpan>());
        if (failure == "playtime")
        {
            config.Setup(x => x.AddPlaytime(It.IsAny<string>(), It.IsAny<TimeSpan>())).Throws(new IOException("Database unavailable"));
        }

        using var factory = ObjectCreationHelper.CreateInstalledAddonsProviderFactory(config.Object);
        var port = new Mock<BasePort> { CallBase = true };
        port.SetupGet(x => x.PortEnum).Returns(failure == "args" ? PortEnum.EDuke32 : PortEnum.Stub);
        port.SetupGet(x => x.CmdArguments).Returns(new StubPort().CmdArguments);
        if (failure == "setup")
        {
            port.Setup(x => x.BeforeStart(It.IsAny<BaseGame>(), It.IsAny<BaseAddon>())).Throws(new IOException("Setup failed"));
        }

        if (failure is "cleanup" or "start-cleanup")
        {
            port.Setup(x => x.AfterEnd(It.IsAny<BaseGame>(), It.IsAny<BaseAddon>())).Throws(new IOException("Cleanup failed"));
        }

        var process = new Mock<IProcessRunResult>();
        var runner = new Mock<IProcessRunner>();
        runner.Setup(x => x.Start(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(failure == "null" ? null : process.Object);
        if (failure is "start" or "start-cleanup")
        {
            runner.Setup(x => x.Start(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new IOException("Start failed"));
        }

        runner.Setup(x => x.WaitForExitAsync(It.IsAny<IProcessRunResult>(), It.IsAny<CancellationToken>())).Returns(failure == "wait" ? Task.FromException(new IOException("Wait failed")) : Task.CompletedTask);
        var starter = new PortStarter(new PlaytimeProvider(config.Object), factory, runner.Object, NullLogger<PortStarter>.Instance);
        var game = new DukeGame { Duke64RomPath = null, DukeZHRomPath = null, DukeWTInstallPath = null };
        var addon = CampaignTestFactory.CreateDukeCampaign("test", GameEnum.Duke3D, null, failure == "args" ? AddonTypeEnum.Mod : AddonTypeEnum.TC);
        if (failure == "args")
        {
            addon = CampaignTestFactory.CreateDukeCampaign("test", GameEnum.Duke3D, new Core.Client.Helpers.AddonFilePathWrapper("test.zip", "addon.json"), AddonTypeEnum.Mod);
        }

        var result = await starter.StartAsync(port.Object, game, addon, [], null, false, false, Path.Combine(Path.GetTempPath(), "test.exe"));

        Assert.Equal(failure == "success", result.IsSuccess);
        port.Verify(x => x.AfterEnd(game, addon), failure == "setup" ? Times.Never() : Times.Once());
        var started = failure is "success" or "wait" or "playtime" or "cleanup";
        process.Verify(x => x.Dispose(), started ? Times.Once() : Times.Never());
        var recorded = failure is "success" or "playtime" or "cleanup";
        config.Verify(x => x.AddPlaytime("test", It.IsAny<TimeSpan>()), recorded ? Times.Once() : Times.Never());
        if (!result.IsSuccess)
        {
            Assert.Contains("test.exe", result.Message, StringComparison.Ordinal);
            if (failure == "start-cleanup")
            {
                Assert.Contains("Start failed", result.Message, StringComparison.Ordinal);
                Assert.Contains("Cleanup failed", result.Message, StringComparison.Ordinal);
            }
        }
    }

    /// <summary> Cleanup, handle disposal and playtime persistence happen only after the process wait completes. </summary>
    [Fact]
    public async Task StartAsync_RunningProcess_DefersCleanupAndPlaytime()
    {
        var config = new Mock<IConfigProvider>();
        config.SetupGet(x => x.Playtimes).Returns(new Dictionary<string, TimeSpan>());
        using var factory = ObjectCreationHelper.CreateInstalledAddonsProviderFactory(config.Object);
        var port = new Mock<BasePort> { CallBase = true };
        port.SetupGet(x => x.PortEnum).Returns(PortEnum.Stub);
        port.SetupGet(x => x.CmdArguments).Returns(new StubPort().CmdArguments);
        var process = new Mock<IProcessRunResult>();
        var runner = new Mock<IProcessRunner>();
        var exit = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        runner.Setup(x => x.Start(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(process.Object);
        runner.Setup(x => x.WaitForExitAsync(process.Object, It.IsAny<CancellationToken>())).Returns(exit.Task);
        var starter = new PortStarter(new PlaytimeProvider(config.Object), factory, runner.Object, NullLogger<PortStarter>.Instance);
        var game = new DukeGame { Duke64RomPath = null, DukeZHRomPath = null, DukeWTInstallPath = null };
        var addon = CampaignTestFactory.CreateDukeCampaign("running", GameEnum.Duke3D, null);
        var operation = starter.StartAsync(port.Object, game, addon, [], null, false, false, Path.Combine(Path.GetTempPath(), "running.exe"));
        try
        {
            Assert.False(operation.IsCompleted);
            port.Verify(x => x.AfterEnd(game, addon), Times.Never());
            process.Verify(x => x.Dispose(), Times.Never());
            config.Verify(x => x.AddPlaytime(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never());
        }
        finally
        {
            exit.TrySetResult();
            await operation;
        }

        Assert.True((await operation).IsSuccess);
        port.Verify(x => x.AfterEnd(game, addon), Times.Once());
        process.Verify(x => x.Dispose(), Times.Once());
        config.Verify(x => x.AddPlaytime("running", It.IsAny<TimeSpan>()), Times.Once());
    }
}
