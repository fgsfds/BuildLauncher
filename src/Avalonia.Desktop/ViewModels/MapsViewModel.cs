using Common.Client.Config;
using Common.Client.Providers;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Mods.Providers;
using Ports.Ports;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class MapsViewModel : RightPanelViewModel, IPortsButtonControl
{
    public readonly IGame Game;

    private readonly GamesProvider _gamesProvider;
    private readonly IConfigProvider _config;
    private readonly PlaytimeProvider _playtimeProvider;
    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly DownloadableAddonsProvider _downloadableAddonsProvider;
    private readonly ILogger _logger;


    /// <summary>
    /// VM initialization
    /// </summary>
    public Task InitializeAsync() => UpdateAsync(false);

    /// <summary>
    /// Update maps list
    /// </summary>
    private async Task UpdateAsync(bool createNew)
    {
        IsInProgress = true;
        await _installedAddonsProvider.CreateCache(createNew).ConfigureAwait(true);
        IsInProgress = false;
    }


    #region Binding Properties

    /// <summary>
    /// List of installed maps
    /// </summary>
    public ImmutableList<IAddon> MapsList
    {
        get
        {
            var result = _installedAddonsProvider.GetInstalledMaps().Select(static x => x.Value).OrderBy(static x => x.Title);

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
            OnPropertyChanged(nameof(SelectedAddonRating));
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

    /// <summary>
    /// Is form in progress
    /// </summary>
    [ObservableProperty]
    private bool _isInProgress;

    public bool IsPortsButtonsVisible => true;

    #endregion


    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public MapsViewModel(
        IGame game,
        GamesProvider gamesProvider,
        IConfigProvider config,
        PlaytimeProvider playtimeProvider,
        RatingProvider ratingProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory _downloadableAddonsProviderFactory,
        ILogger logger
        ) : base(playtimeProvider, ratingProvider)
    {
        Game = game;

        _gamesProvider = gamesProvider;
        _config = config;
        _playtimeProvider = playtimeProvider;
        _installedAddonsProvider = installedAddonsProviderFactory.GetSingleton(game);
        _downloadableAddonsProvider = _downloadableAddonsProviderFactory.GetSingleton(game);
        _logger = logger;

        _gamesProvider.GameChangedEvent += OnGameChanged;
        _installedAddonsProvider.AddonsChangedEvent += OnAddonChanged;
        _downloadableAddonsProvider.AddonDownloadedEvent += OnAddonChanged;
    }


    #region Relay Commands

    /// <summary>
    /// Start selected map
    /// </summary>
    /// <param name="command">Port to start map with</param>
    [RelayCommand]
    private async Task StartMapAsync(object? command)
    {
        try
        {
            command.ThrowIfNotType<Tuple<BasePort, byte?>>(out var parameter);
            Guard.IsNotNull(SelectedAddon);

            var mods = _installedAddonsProvider.GetInstalledMods();

            var args = parameter.Item1.GetStartGameArgs(Game, SelectedAddon, mods, _config.SkipIntro, _config.SkipStartup, parameter.Item2);

            _logger.LogInformation($"=== Starting map {SelectedAddon.Id} for {Game.FullName} ===");
            _logger.LogInformation($"Path to port exe {parameter.Item1.PortExeFilePath}");
            _logger.LogInformation($"Startup args: {args}");

            await StartPortAsync(SelectedAddon.Id, parameter.Item1.PortExeFilePath, args).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Critical error ===");
        }
    }


    /// <summary>
    /// Open maps folder
    /// </summary>
    [RelayCommand]
    private void OpenFolder()
    {
        _ = Process.Start(new ProcessStartInfo
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
        await UpdateAsync(true).ConfigureAwait(true);
    }


    /// <summary>
    /// Delete selected map
    /// </summary>
    [RelayCommand]
    private void DeleteMap()
    {
        Guard.IsNotNull(SelectedAddon);

        _installedAddonsProvider.DeleteAddon(SelectedAddon);
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
    /// <param name="id">Map id</param>
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
        })!.WaitForExitAsync().ConfigureAwait(true);

        sw.Stop();
        var time = sw.Elapsed;

        _playtimeProvider.AddTime(id, time);

        OnPropertyChanged(nameof(SelectedAddonPlaytime));
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
