using Addons.Providers;
using Avalonia.Controls;
using Avalonia.Desktop.Controls;
using Avalonia.Desktop.ViewModels;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Core.All.Enums;
using Core.All.Serializable.Downloadable;
using Core.Client.Cache;
using Core.Client.Config;
using Core.Client.Interfaces;
using Games.Games;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tests.Unit.Sequential.Helpers;

namespace Tests.Unit.Sequential;

/// <summary> Exercises catalog filtering, selection and refresh through production download controls. </summary>
public sealed class DownloadsControlTests
{
    /// <summary> Search and type filters update rendered rows and the clear button restores them. </summary>
    [AvaloniaFact]
    public async Task SearchAndFilter_UpdateVisibleCatalogAsync()
    {
        HeadlessView.Prepare();
        using var fixture = new Catalog();
        await fixture.ViewModel.UpdateCommand.ExecuteAsync(false);
        var control = new DownloadsControl { DataContext = fixture.ViewModel };
        using var host = new HeadlessView(control);
        var grid = host.Window.GetVisualDescendants().OfType<DataGrid>().Single();
        Assert.Equal(3, grid.ItemsSource.Cast<object>().Count());
        control.FindControl<TextBox>("SearchBox")!.Text = "ALPHA";
        Assert.Equal("Alpha campaign", Assert.Single(grid.ItemsSource.Cast<DownloadableAddonJsonModel>()).Title);
        host.Click(host.Window.GetVisualDescendants().OfType<Button>().Single(x => ToolTip.GetTip(x)?.ToString() == "Clear search"));
        Assert.Equal(3, grid.ItemsSource.Cast<object>().Count());
        host.Window.GetVisualDescendants().OfType<ComboBox>().Single().SelectedItem = DownloadsViewModel.FilterItemEnum.Maps;
        Assert.Equal(AddonTypeEnum.Map, Assert.Single(grid.ItemsSource.Cast<DownloadableAddonJsonModel>()).AddonType);
        control.FindControl<TextBox>("SearchBox")!.Text = "no matching addon";
        Assert.Empty(grid.ItemsSource.Cast<object>());
    }

    /// <summary> Multiple selected rows update the bound selection and download size, and refresh clears them. </summary>
    [AvaloniaFact]
    public async Task SelectionAndRefresh_UpdateDownloadSummaryAsync()
    {
        HeadlessView.Prepare();
        using var fixture = new Catalog();
        await fixture.ViewModel.UpdateCommand.ExecuteAsync(false);
        var control = new DownloadsControl { DataContext = fixture.ViewModel };
        using var host = new HeadlessView(control);
        var grid = host.Window.GetVisualDescendants().OfType<DataGrid>().Single();
        var rows = grid.ItemsSource.Cast<DownloadableAddonJsonModel>().Take(2).ToArray();
        grid.SelectedItems.Add(rows[0]);
        grid.SelectedItems.Add(rows[1]);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(rows, fixture.ViewModel.SelectedDownloads);
        Assert.Equal("Download (2KB)", control.FindControl<Button>("DownloadButton")!.Content);
        var pending = new TaskCompletionSource<IReadOnlyList<DownloadableAddonJsonModel>?>(TaskCreationOptions.RunContinuationsAsynchronously);
        fixture.Api.Setup(x => x.GetAddonsAsync(GameEnum.Duke3D, It.IsAny<CancellationToken>())).Returns(pending.Task);
        host.Click(host.Window.GetVisualDescendants().OfType<Button>().Single(x => ToolTip.GetTip(x)?.ToString() == "Refresh"));
        Assert.True(control.FindControl<Button>("CancelButton")!.IsVisible);
        Assert.False(control.FindControl<Button>("DownloadButton")!.IsVisible);
        pending.SetResult(rows);
        await fixture.ViewModel.UpdateCommand.ExecutionTask!;
        Dispatcher.UIThread.RunJobs();
        Assert.Empty(fixture.ViewModel.SelectedDownloads);
        Assert.Empty(grid.SelectedItems);
        Assert.Equal("Download", control.FindControl<Button>("DownloadButton")!.Content);
        Assert.False(control.FindControl<Button>("CancelButton")!.IsVisible);
    }

    /// <summary> Owns the isolated catalog services used by each view. </summary>
    private sealed class Catalog : IDisposable
    {
        /// <summary> Supplies catalog responses without network access. </summary>
        public Mock<IApiInterface> Api { get; } = new();
        /// <summary> The bound production view model. </summary>
        public DownloadsViewModel ViewModel { get; }
        /// <summary> Metadata owned by this fixture. </summary>
        private readonly MetadataProvider _metadata;
        /// <summary> Installed-content providers owned by this fixture. </summary>
        private readonly InstalledAddonsProviderFactory _installed;

        /// <summary> Creates three distinct addon kinds with deterministic sizes. </summary>
        public Catalog()
        {
            var config = new ConfigProviderFake();
            _metadata = new MetadataProvider(Api.Object, NullLogger<MetadataProvider>.Instance);
            _installed = new InstalledAddonsProviderFactory(config, Mock.Of<ICacheAdder<Stream>>(),
                new OriginalCampaignsProvider(config), _metadata, NullLoggerFactory.Instance);
            var game = new DukeGame { GameInstallFolder = null, Duke64RomPath = null, DukeZHRomPath = null, DukeWTInstallPath = null };
            Api.Setup(x => x.GetAddonsAsync(GameEnum.Duke3D, It.IsAny<CancellationToken>())).ReturnsAsync(
            [
                Entry("Alpha campaign", AddonTypeEnum.TC), Entry("Beta map", AddonTypeEnum.Map), Entry("Gamma mod", AddonTypeEnum.Mod)
            ]);
            var downloads = new DownloadableAddonsProviderFactory(null!, Api.Object, null!, _installed, NullLoggerFactory.Instance);
            ViewModel = new DownloadsViewModel(game, _installed, downloads, NullLogger<DownloadsViewModel>.Instance);
        }

        /// <summary> Creates catalog metadata without a downloadable payload. </summary>
        /// <param name="title">The addon title.</param>
        /// <param name="type">The addon kind.</param>
        /// <returns>The catalog entry.</returns>
        private static DownloadableAddonJsonModel Entry(string title, AddonTypeEnum type) => new()
        {
            Id = title, Title = title, AddonType = type, Game = GameEnum.Duke3D,
            Version = "1.0", FileSize = 1024, DownloadUrl = new Uri("https://example.invalid/addon.zip")
        };

        /// <summary> Releases subscriptions and owned providers. </summary>
        public void Dispose()
        {
            ViewModel.Dispose();
            _installed.Dispose();
            _metadata.Dispose();
        }
    }
}
