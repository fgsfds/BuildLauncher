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

public sealed partial class MapsViewModel : RightPanelViewModel, IPortsButtonControl
{
    public readonly IGame Game;

    private readonly GamesProvider _gamesProvider;
    private readonly IConfigProvider _config;
    private readonly PlaytimeProvider _playtimeProvider;
    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly DownloadableAddonsProvider _downloadableAddonsProvider;


    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public MapsViewModel(
        IGame game,
        GamesProvider gamesProvider,
        IConfigProvider config,
        PlaytimeProvider playtimeProvider,
        ApiInterface apiInterface,
        ScoresProvider scoresProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory _downloadableAddonsProviderFactory
        ) : base(config, playtimeProvider, apiInterface, scoresProvider)
    {
        Game = game;

        _gamesProvider = gamesProvider;
        _config = config;
        _playtimeProvider = playtimeProvider;
        _installedAddonsProvider = installedAddonsProviderFactory.GetSingleton(game);
        _downloadableAddonsProvider = _downloadableAddonsProviderFactory.GetSingleton(game);

        _gamesProvider.GameChangedEvent += OnGameChanged;
        _installedAddonsProvider.AddonsChangedEvent += OnAddonChanged;
        _downloadableAddonsProvider.AddonDownloadedEvent += OnAddonChanged;
    }


    /// <summary>
    /// VM initialization
    /// </summary>
    public Task InitializeAsync() => UpdateAsync(false);

    /// <summary>
    /// Update maps list
    /// </summary>
    private async Task UpdateAsync(bool createNew)
    {
        await _installedAddonsProvider.CreateCache(createNew);

        OnPropertyChanged(nameof(MapsList));
    }


    #region Binding Properties

    /// <summary>
    /// List of installed maps
    /// </summary>
    public ImmutableList<IAddon> MapsList
    {
        get
        {
            var result = _installedAddonsProvider.GetInstalledMaps().Select(static x => x.Value);

            if (string.IsNullOrWhiteSpace(SearchBoxText))
            {
                return [.. result];
            }

            return [.. result.Where(x => x.Title.Contains(SearchBoxText, StringComparison.CurrentCultureIgnoreCase))];
        }
    }

    private IAddon? _selectedAddon;
    /// <summary>
    /// Currently selected map
    /// </summary>
    public override IAddon? SelectedAddon
    {
        get => _selectedAddon;
        set
        {
            _selectedAddon = value;

            OnPropertyChanged(nameof(SelectedAddonDescription));
            OnPropertyChanged(nameof(SelectedAddonScore));
            OnPropertyChanged(nameof(IsSelectedAddonUpvoted));
            OnPropertyChanged(nameof(IsSelectedAddonDownvoted));
            OnPropertyChanged(nameof(SelectedAddonPlaytime));
            OnPropertyChanged(nameof(SelectedAddonPreview));
            OnPropertyChanged(nameof(IsPreviewVisible));

            StartMapCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Search box text
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MapsList))]
    [NotifyCanExecuteChangedFor(nameof(ClearSearchBoxCommand))]
    private string _searchBoxText;

    public bool IsPortsButtonsVisible => true;

    #endregion


    #region Relay Commands

    /// <summary>
    /// Start selected map
    /// </summary>
    /// <param name="command">Port to start map with</param>
    [RelayCommand]
    private async Task StartMapAsync(object? command)
    {
        command.ThrowIfNotType<Tuple<BasePort, byte?>>(out var parameter);
        SelectedAddon.ThrowIfNull();

        var mods = _installedAddonsProvider.GetInstalledMods();

        var args = parameter.Item1.GetStartGameArgs(Game, SelectedAddon, mods, _config.SkipIntro, _config.SkipStartup, parameter.Item2);

        await StartPortAsync(SelectedAddon.Id, parameter.Item1.FullPathToExe, args);
    }


    /// <summary>
    /// Open maps folder
    /// </summary>
    [RelayCommand]
    private void OpenFolder()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = Game.MapsFolderPath,
            UseShellExecute = true,
        });
    }


    /// <summary>
    /// Refresh maps list
    /// </summary>
    [RelayCommand]
    private async Task RefreshListAsync()
    {
        await UpdateAsync(true);
    }


    /// <summary>
    /// Delete selected map
    /// </summary>
    [RelayCommand]
    private void DeleteMap()
    {
        SelectedAddon.ThrowIfNull();

        _installedAddonsProvider.DeleteAddon(SelectedAddon);

        OnPropertyChanged(nameof(MapsList));
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

        OnPropertyChanged(nameof(SelectedAddonDescription));
    }


    private void OnGameChanged(GameEnum parameterName)
    {
        if (parameterName == Game.GameEnum)
        {
            OnPropertyChanged(nameof(MapsList));
        }
    }

    private void OnAddonChanged(IGame game, AddonTypeEnum? addonType)
    {
        if (game.GameEnum == Game.GameEnum && (addonType is AddonTypeEnum.Map || addonType is null))
        {
            OnPropertyChanged(nameof(MapsList));
        }
    }
}
