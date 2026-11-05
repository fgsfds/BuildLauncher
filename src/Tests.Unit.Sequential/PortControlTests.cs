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
using Ports.Ports;
using Tests.Unit.Sequential.Helpers;

namespace Tests.Unit.Sequential;

/// <summary> Verifies port cards display release availability and installation actions through real bindings. </summary>
public sealed class PortControlTests
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
        api.Setup(x => x.GetLatestPortReleaseAsync(It.IsAny<PortEnum>(), It.IsAny<CancellationToken>())).Returns(pending.Task);
        var model = new StubPort();
        var vm = new PortViewModel(null!, api.Object, model, NullLogger<PortViewModel>.Instance);
        var control = new PortControl { DataContext = vm };
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
        Assert.True(control.FindControl<Button>("OpenFolderButton")!.IsEffectivelyEnabled);
    }
}
