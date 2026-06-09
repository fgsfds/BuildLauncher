using System.Collections.Immutable;
using System.Diagnostics;
using Addons.Addons;
using Addons.Providers;
using Avalonia.Controls.Notifications;
using Avalonia.Desktop.Misc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.All.Enums;
using Core.Client.Interfaces;
using Core.Client.Providers;
using Games.Games;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Ports.Ports;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class MapsViewModel : RightPanelViewModel, IPortsButtonControl
{
    public BaseGame Game { get; }

    private readonly InstalledGamesProvider _gamesProvider;
    private readonly IConfigProvider _config;
    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly IAddonDropHelper _addonInstaller;
    private readonly DownloadableAddonsProvider _downloadableAddonsProvider;
    private readonly PortStarter _portStarter;
    private readonly MetadataProvider _metadataProvider;
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
        await _installedAddonsProvider.CreateCache(createNew, AddonTypeEnum.Map).ConfigureAwait(true);
        IsInProgress = false;
    }


    #region Binding Properties

    /// <summary>
    /// List of installed maps
    /// </summary>
    public ImmutableList<BaseAddon> MapsList
    {
        get
        {
            var result = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map).Select(static x => x.Value).OrderBy(static x => x.Title);

            if (string.IsNullOrWhiteSpace(SearchBoxText))
            {
                return [.. result];
            }

            return [.. result.Where(x => x.Title.Contains(SearchBoxText, StringComparison.CurrentCultureIgnoreCase))];
        }
    }

    private BaseAddon? _selectedAddon;
    /// <summary>
    /// Currently selected map
    /// </summary>
    public override BaseAddon? SelectedAddon
    {
        get => _selectedAddon;
        set
        {
            _selectedAddon = value;

            OnPropertyChanged(nameof(SelectedAddonDescription));
            OnPropertyChanged(nameof(SelectedAddonRating));
            OnPropertyChanged(nameof(IsMetadataUpdateAvailable));
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
    private string _searchBoxText = string.Empty;

    /// <summary>
    /// Is form in progress
    /// </summary>
    [ObservableProperty]
    private bool _isInProgress;

    public bool IsPortsButtonsVisible => true;

    #endregion


    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public MapsViewModel(
        BaseGame game,
        InstalledGamesProvider gamesProvider,
        IConfigProvider config,
        PlaytimeProvider playtimeProvider,
        RatingProvider ratingProvider,
        MetadataProvider metadataProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory downloadableAddonsProviderFactory,
        PortStarter portStarter,
        BitmapsCache bitmapsCache,
        IAddonDropHelper addonInstaller,
        ILogger logger
        ) : base(playtimeProvider, ratingProvider, metadataProvider, bitmapsCache, config)
    {
        Game = game;

        _gamesProvider = gamesProvider;
        _config = config;
        _installedAddonsProvider = installedAddonsProviderFactory.Get(game);
        _downloadableAddonsProvider = downloadableAddonsProviderFactory.Get(game);
        _portStarter = portStarter;
        _metadataProvider = metadataProvider;
        _addonInstaller = addonInstaller;
        _logger = logger;

        _gamesProvider.GameChangedEvent += OnGameChanged;
        _installedAddonsProvider.AddonsChangedEvent += OnAddonChanged;
        _downloadableAddonsProvider.AddonsChangedEvent += OnAddonChanged;
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
            if (command is not Tuple<BasePort, byte?> parameter)
            {
                throw new InvalidCastException();
            }

            ArgumentNullException.ThrowIfNull(SelectedAddon);

            await _portStarter.StartAsync(
                parameter.Item1,
                Game,
                SelectedAddon,
                [],
                parameter.Item2,
                _config.SkipIntro,
                _config.SkipStartup).ConfigureAwait(true);

            OnPropertyChanged(nameof(SelectedAddonPlaytime));
        }
        catch (Exception ex)
        {
            NotificationsHelper.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, $"=== Error while starting map {SelectedAddon?.Title} ===");
        }
    }


    /// <summary>
    /// Open maps folder
    /// </summary>
    [RelayCommand]
    private void OpenFolder()
    {
        using var process = Process.Start(new ProcessStartInfo
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
        ArgumentNullException.ThrowIfNull(SelectedAddon);

        _installedAddonsProvider.DeleteAddon(SelectedAddon);
    }


    /// <summary>
    /// Clear search bar
    /// </summary>
    [RelayCommand(CanExecute = nameof(ClearSearchBoxCanExecute))]
    private void ClearSearchBox() => SearchBoxText = string.Empty;
    private bool ClearSearchBoxCanExecute() => !string.IsNullOrEmpty(SearchBoxText);


    /// <summary>
    /// Install dropped addon
    /// </summary>
    [RelayCommand]
    private Task ProcessDroppedFilesAsync(List<string> filePaths) => _addonInstaller.AddAddonsAsync(filePaths, Game);


    /// <summary>
    /// Updates metadata for selected map.
    /// </summary>
    public override async Task UpdateMetadataAsync(object? value)
    {
        if (value is not null
            && value is BaseAddon addon)
        {
        }
        else if (value is null
            && SelectedAddon is not null)
        {
            addon = SelectedAddon;
        }
        else
        {
            throw new InvalidOperationException(value?.GetType().Name);
        }

        IsInProgress = true;

        var result = await _metadataProvider.UpdateMetadataAsync(addon.PathToFile).ConfigureAwait(true);

        IsInProgress = false;

        if (result.IsSuccess)
        {
            NotificationsHelper.Show(
                "Metadata updated.",
                NotificationType.Success
                );
        }
        else
        {
            NotificationsHelper.Show(
                "Error while updating metadata.",
                NotificationType.Error
                );
        }
    }

    #endregion


    private void OnGameChanged(GameEnum parameterName)
    {
        if (parameterName == Game.GameEnum)
        {
            OnPropertyChanged(nameof(MapsList));
        }
    }

    private void OnAddonChanged(GameEnum gameEnum, AddonTypeEnum? addonType)
    {
        if (gameEnum == Game.GameEnum && (addonType is AddonTypeEnum.Map))
        {
            OnPropertyChanged(nameof(MapsList));
        }
    }
}
