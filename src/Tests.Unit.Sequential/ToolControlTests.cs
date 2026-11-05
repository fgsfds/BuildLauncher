using Avalonia.Controls;
using Avalonia.Desktop.Controls;
using Avalonia.Desktop.ViewModels;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Core.All.Enums;
using Core.All.Helpers;
using Core.All.Serializable.Downloadable;
using Core.Client.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tools.Tools;
using Tests.Unit.Sequential.Helpers;

namespace Tests.Unit.Sequential;

/// <summary> Verifies tool cards display release availability and installation actions through real bindings. </summary>
public sealed class ToolControlTests
{
    /// <summary> An install button becomes available only after a release is returned; missing releases keep it disabled. </summary>
    /// <param name="available">Whether the release service returns an installable release.</param>
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReleaseLookup_UpdatesInstallAvailabilityAsync(bool available)
    {
        HeadlessView.Prepare();
        var api = new Mock<IApiInterface>();
        var pending = new TaskCompletionSource<GeneralReleaseJsonModel?>(TaskCreationOptions.RunContinuationsAsynchronously);
        api.Setup(x => x.GetLatestToolReleaseAsync(It.IsAny<ToolEnum>(), It.IsAny<CancellationToken>())).Returns(pending.Task);
        var model = new TestTool();
        var vm = new ToolViewModel(null!, api.Object, model, NullLogger<ToolViewModel>.Instance);
        var control = new ToolControl { DataContext = vm };
        using var host = new HeadlessView(control);
        var install = control.FindControl<Button>("InstallButton")!;
        Assert.False(install.IsEffectivelyEnabled);
        var initialize = vm.InitializeAsync();
        Assert.True(host.Window.GetVisualDescendants().OfType<ProgressBar>().Single().IsIndeterminate);
        pending.SetResult(available ? new GeneralReleaseJsonModel
        {
            Version = "2.0", SupportedOS = CommonProperties.OSEnum, Description = "Test release",
            DownloadUrl = new Uri("https://example.invalid/release.zip"), Hash = null
        } : null);
        await initialize;
        Assert.Equal(available, install.IsEffectivelyEnabled);
        Assert.False(host.Window.GetVisualDescendants().OfType<ProgressBar>().Single().IsIndeterminate);
        Assert.Contains(host.Window.GetVisualDescendants().OfType<TextBlock>(), x => x.Text == (available ? "2.0" : "Error"));
        Assert.False(control.FindControl<Button>("OpenFolderButton")!.IsEffectivelyEnabled);
    }

    /// <summary> Supplies deterministic tool capabilities without installed files or native processes. </summary>
    private sealed class TestTool : BaseTool
    {
        /// <inheritdoc />
        public override ToolEnum ToolEnum => ToolEnum.Mapster32;
        /// <inheritdoc />
        protected override string WinExe => "test-tool.exe";
        /// <inheritdoc />
        protected override string LinExe => "test-tool";
        /// <inheritdoc />
        public override string Name => "Test tool";
        /// <inheritdoc />
        public override bool CanBeLaunched => false;
        /// <inheritdoc />
        public override string? InstalledVersion => null;
        /// <inheritdoc />
        public override string GetStartToolArgs() => string.Empty;
    }
}
