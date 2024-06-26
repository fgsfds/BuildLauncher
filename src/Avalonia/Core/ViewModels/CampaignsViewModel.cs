using ClientCommon.API;
using ClientCommon.Config;
using ClientCommon.Providers;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Games.Providers;
using Mods.Providers;
using Ports.Ports;
using System.Collections.Immutable;
using System.Diagnostics;

namespace BuildLauncher.ViewModels;

public sealed partial class CampaignsViewModel : RightPanelViewModel, IPortsButtonControl
{
    public readonly IGame Game;

    private readonly GamesProvider _gamesProvider;
    private readonly IConfigProvider _config;
    private readonly PlaytimeProvider _playtimeProvider;
    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly DownloadableAddonsProvider _downloadableAddonsProvider;


    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public CampaignsViewModel(
        IGame game,
        GamesProvider gamesProvider,
        IConfigProvider config,
        PlaytimeProvider playtimeProvider,
        ApiInterface apiInterface,
        RatingProvider ratingProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory downloadableAddonsProviderFactory
        ) : base(config, playtimeProvider, apiInterface, ratingProvider)
    {
        Game = game;

        _gamesProvider = gamesProvider;
        _config = config;
        _playtimeProvider = playtimeProvider;
        _installedAddonsProvider = installedAddonsProviderFactory.GetSingleton(game);
        _downloadableAddonsProvider = downloadableAddonsProviderFactory.GetSingleton(game);

        _gamesProvider.GameChangedEvent += OnGameChanged;
        _installedAddonsProvider.AddonsChangedEvent += OnAddonChanged;
        _downloadableAddonsProvider.AddonDownloadedEvent += OnAddonChanged;
    }


    #region Binding Properties

    /// <summary>
    /// List of installed campaigns and maps
    /// </summary>
    public ImmutableList<IAddon> CampaignsList
    {
        get
        {
            var result = _installedAddonsProvider.GetInstalledCampaigns().Select(static x => x.Value);

            if (string.IsNullOrWhiteSpace(SearchBoxText))
            {
                return [.. result];
            }

            return [.. result.Where(x => x.Title.Contains(SearchBoxText, StringComparison.CurrentCultureIgnoreCase))];
        }
    }

    private IAddon? _selectedAddon;
    /// <summary>
    /// Currently selected campaign
    /// </summary>
    public override IAddon? SelectedAddon
    {
        get => _selectedAddon;
        set
        {
            _selectedAddon = value;

            OnPropertyChanged(nameof(SelectedAddonDescription));
            OnPropertyChanged(nameof(SelectedAddonPreview));
            OnPropertyChanged(nameof(SelectedAddonRating));
            OnPropertyChanged(nameof(SelectedAddonPlaytime));
            OnPropertyChanged(nameof(IsPreviewVisible));

            StartCampaignCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Search box text
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CampaignsList))]
    [NotifyCanExecuteChangedFor(nameof(ClearSearchBoxCommand))]
    private string _searchBoxText;

    public bool IsPortsButtonsVisible => true;

    #endregion


    /// <summary>
    /// VM initialization
    /// </summary>
    public Task InitializeAsync() => UpdateAsync(false);

    /// <summary>
    /// Update campaigns list
    /// </summary>
    private async Task UpdateAsync(bool createNew)
    {
        await _installedAddonsProvider.CreateCache(createNew);

        OnPropertyChanged(nameof(CampaignsList));
    }


    #region Relay Commands

    /// <summary>
    /// Start selected campaign
    /// </summary>
    /// <param name="command">Port to start campaign with</param>
    [RelayCommand]
    private async Task StartCampaignAsync(object? command)
    {
        command.ThrowIfNotType<BasePort>(out var port);
        SelectedAddon.ThrowIfNull();

        var mods = _installedAddonsProvider.GetInstalledMods();

        var args = port.GetStartGameArgs(Game, SelectedAddon, mods, _config.SkipIntro, _config.SkipStartup);

        await StartPortAsync(SelectedAddon.Id, port.FullPathToExe, args);
    }


    /// <summary>
    /// Open campaigns folder
    /// </summary>
    [RelayCommand]
    private void OpenFolder()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = Game.CampaignsFolderPath,
            UseShellExecute = true,
        });
    }


    /// <summary>
    /// Refresh campaigns list
    /// </summary>
    [RelayCommand]
    private async Task RefreshListAsync()
    {
        await UpdateAsync(true);
    }


    /// <summary>
    /// Delete selected campaign
    /// </summary>
    [RelayCommand]
    private void DeleteCampaign()
    {
        SelectedAddon.ThrowIfNull();

        _installedAddonsProvider.DeleteAddon(SelectedAddon);

        OnPropertyChanged(nameof(CampaignsList));
    }


    /// <summary>
    /// Clear search bar
    /// </summary>
    [RelayCommand(CanExecute = nameof(ClearSearchBoxCanExecute))]
    private void ClearSearchBox() => SearchBoxText = string.Empty;
    private bool ClearSearchBoxCanExecute() => !string.IsNullOrEmpty(SearchBoxText);

    #endregion


    /// <summary>
    /// Start port with command line args
    /// </summary>
    /// <param name="exe">Path to port exe</param>
    /// <param name="args">Command line arguments</param>
    private async Task StartPortAsync(string id, string exe, string args)
    {
        var sw = Stopwatch.StartNew();

        await Process.Start(new ProcessStartInfo
        {
            FileName = exe,
            UseShellExecute = true,
            Arguments = args,
            WorkingDirectory = Path.GetDirectoryName(exe)
        })!.WaitForExitAsync();

        sw.Stop();
        var time = sw.Elapsed;

        _playtimeProvider.AddTime(id, time);

        OnPropertyChanged(nameof(SelectedAddonPlaytime));
    }


    private void OnGameChanged(GameEnum parameterName)
    {
        if (parameterName == Game.GameEnum)
        {
            OnPropertyChanged(nameof(CampaignsList));
        }
    }

    private void OnAddonChanged(IGame game, AddonTypeEnum? addonType)
    {
        if (game.GameEnum == Game.GameEnum && (addonType is AddonTypeEnum.TC || addonType is null))
        {
            OnPropertyChanged(nameof(CampaignsList));
        }
    }
}
