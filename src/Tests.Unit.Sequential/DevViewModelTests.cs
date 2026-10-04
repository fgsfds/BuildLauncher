using System.Text.Json;
using Avalonia.Desktop.ViewModels;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Core.All.Enums;
using Core.All.Serializable.Addon;
using Core.Client.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Tests.Unit;

/// <summary>Verifies the developer preview command uses shared manifest authoring rules.</summary>
public sealed class DevViewModelTests : IDisposable
{
    /// <summary>The isolated addon folder owned by this test instance.</summary>
    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"BuildLauncher-DevViewModelTests-{Guid.NewGuid():N}");

    /// <summary>Creates an isolated folder with uniquely named Blood resource files.</summary>
    public DevViewModelTests()
    {
        _ = Directory.CreateDirectory(_folder);
        File.WriteAllText(Path.Combine(_folder, "campaign.ini"), "[Episode1]\nMap1 = example");
        File.WriteAllText(Path.Combine(_folder, "resources.def"), "");
    }

    /// <summary>Removes only the addon folder owned by this test instance.</summary>
    public void Dispose()
    {
        Directory.Delete(_folder, recursive: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Serializes a valid campaign through the public preview command.</summary>
    [AvaloniaFact]
    public void PreviewJsonCommand_ValidBloodFolder_ProducesManifestWithoutWritingFiles()
    {
        var viewModel = CreateViewModel();

        viewModel.PreviewJsonCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Null(viewModel.ErrorText);
        Assert.NotNull(viewModel.JsonText);
        var manifest = JsonSerializer.Deserialize(viewModel.JsonText, AddonManifestJsonContext.Default.AddonManifestJsonModel);
        Assert.NotNull(manifest);
        Assert.Equal("blood-example", manifest.Id);
        Assert.Equal(GameEnum.Blood, manifest.SupportedGame.Game);
        Assert.Equal("campaign.ini", manifest.Ini);
        Assert.Equal(new[] { "resources.def" }, manifest.AdditionalDefs);
        Assert.False(File.Exists(Path.Combine(_folder, "addon.json")));
    }

    /// <summary>Surfaces the shared Blood ART diagnostic without changing source files.</summary>
    [AvaloniaFact]
    public void PreviewJsonCommand_RootBloodArt_ShowsSharedValidationError()
    {
        var artPath = Path.Combine(_folder, "TILES001.aRt");
        var original = new byte[] { 1, 2, 3 };
        File.WriteAllBytes(artPath, original);
        var viewModel = CreateViewModel();

        viewModel.PreviewJsonCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("ART files are not supported in Blood addons. Convert them to DEF.", viewModel.ErrorText);
        Assert.Null(viewModel.JsonText);
        Assert.Equal(original, File.ReadAllBytes(artPath));
        Assert.False(File.Exists(Path.Combine(_folder, "addon.json")));
    }

    /// <summary>Retains the package-file exception for an actual Linux executable override.</summary>
    [AvaloniaFact]
    public void PreviewJsonCommand_CustomLinuxExecutable_BypassesPackageFileRules()
    {
        File.WriteAllBytes(Path.Combine(_folder, "TILES001.ART"), [1, 2, 3]);
        var viewModel = CreateViewModel();
        viewModel.LinuxNBloodExe = "bin/custom-nblood";

        viewModel.PreviewJsonCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Null(viewModel.ErrorText);
        Assert.NotNull(viewModel.JsonText);
        var manifest = JsonSerializer.Deserialize(viewModel.JsonText, AddonManifestJsonContext.Default.AddonManifestJsonModel);
        Assert.NotNull(manifest);
        Assert.NotNull(manifest.Executables);
        Assert.Equal("bin/custom-nblood", manifest.Executables[OSEnum.Linux][PortEnum.NBlood]);
        Assert.False(File.Exists(Path.Combine(_folder, "addon.json")));
    }

    /// <summary>Sets the folder before metadata and overrides the title-generated ID last.</summary>
    /// <returns>The view model with complete valid campaign authoring fields.</returns>
    private DevViewModel CreateViewModel()
    {
        var viewModel = new DevViewModel(Mock.Of<IConfigProvider>(), null!, null!, null!, NullLogger<DevViewModel>.Instance)
        {
            PathToAddonFolder = _folder,
            SelectedGame = GameEnum.Blood,
            IsTcSelected = true,
            AddonTitle = "Example Blood campaign",
            AddonVersion = "1.0",
            Ini = "campaign.ini",
            AdditionalDefs = "resources.def"
        };
        viewModel.AddonId = "example";
        return viewModel;
    }
}
