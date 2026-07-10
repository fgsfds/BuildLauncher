using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Addons.Providers;
using Avalonia.Controls.Notifications;
using Avalonia.Desktop.Misc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.All.Enums;
using Core.All.Helpers;
using Core.All.Serializable.Downloadable;
using Games.Games;
using Microsoft.Extensions.Logging;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class DownloadsViewModel : ObservableObject
{
    /// <summary>
    ///     Filter options for the downloadable addons list.
    /// </summary>
    public enum FilterItemEnum
    {
        /// <summary>
        ///     Show all addons.
        /// </summary>
        All,

        /// <summary>
        ///     Show only total conversions.
        /// </summary>
        TCs,

        /// <summary>
        ///     Show only maps.
        /// </summary>
        Maps,

        /// <summary>
        ///     Show only mods.
        /// </summary>
        Mods
    }


    /// <summary>
    ///     The downloadable addons provider.
    /// </summary>
    private readonly DownloadableAddonsProvider _downloadableAddonsProvider;

    /// <summary>
    ///     The installed addons provider.
    /// </summary>
    private readonly InstalledAddonsProvider _installedAddonsProvider;

    private readonly ILogger<DownloadsViewModel> _logger;

    /// <summary>
    ///     The cancellation token source for download operations.
    /// </summary>
    private CancellationTokenSource? _cancellationTokenSource;


    /// <summary>
    ///     Initializes a new instance of the <see cref="DownloadsViewModel" /> class.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <param name="installedAddonsProviderFactory">The installed addons provider factory.</param>
    /// <param name="downloadableAddonsProviderFactory">The downloadable addons provider factory.</param>
    /// <param name="logger">The logger.</param>
    [Obsolete($"Don't create directly. Use {nameof(IViewModelsFactory)}.")]
    public DownloadsViewModel(
        BaseGame game,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory downloadableAddonsProviderFactory,
        ILogger<DownloadsViewModel> logger
        )
    {
        Game = game;

        _installedAddonsProvider = installedAddonsProviderFactory.Get(game);
        _downloadableAddonsProvider = downloadableAddonsProviderFactory.Get(game);
        _logger = logger;

        _installedAddonsProvider.AddonsChangedEvent += OnAddonChanged;
        //_downloadableAddonsProvider.AddonsChangedEvent += OnAddonChanged;
        SelectedDownloads.CollectionChanged += OnSelectedDownloadsChanged;
    }

    /// <summary>
    ///     Gets the game associated with this view model.
    /// </summary>
    public BaseGame Game { get; }


    /// <summary>
    ///     Handles the progress changed event.
    /// </summary>
    private void OnProgressChanged(object? sender, float e)
    {
        ProgressBarValue = e;
        OnPropertyChanged(nameof(ProgressBarValue));
    }

    /// <summary>
    ///     Handles the addon changed event.
    /// </summary>
    private void OnAddonChanged(GameEnum gameEnum, AddonTypeEnum? addonType)
    {
        if (gameEnum != Game.GameEnum)
        {
            return;
        }

        OnPropertyChanged(nameof(DownloadableList));
    }

    /// <summary>
    ///     Handles the selected downloads collection changed event.
    /// </summary>
    private void OnSelectedDownloadsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SelectedDownloadableDescription));
        OnPropertyChanged(nameof(DownloadButtonText));
        DownloadAddonCommand.NotifyCanExecuteChanged();
    }


    #region Binding Properties

    /// <summary>
    ///     Gets whether any downloadable addon has an update available.
    /// </summary>
    public bool HasUpdates => DownloadableList.Any(x => x.IsUpdateAvailable);

    /// <summary>
    ///     List of downloadable addons
    /// </summary>
    public ImmutableList<DownloadableAddonJsonModel> DownloadableList
    {
        get
        {
            IEnumerable<DownloadableAddonJsonModel> result;

            if (FilterSelectedItem is FilterItemEnum.All)
            {
                var unsorted =
                    _downloadableAddonsProvider.GetDownloadableAddons(AddonTypeEnum.TC)
                                               .Concat(_downloadableAddonsProvider.GetDownloadableAddons(AddonTypeEnum.Map))
                                               .Concat(_downloadableAddonsProvider.GetDownloadableAddons(AddonTypeEnum.Mod));

                result = unsorted.OrderBy(static x => x.Title);
            }
            else if (FilterSelectedItem is FilterItemEnum.TCs)
            {
                result = _downloadableAddonsProvider.GetDownloadableAddons(AddonTypeEnum.TC);
            }
            else if (FilterSelectedItem is FilterItemEnum.Maps)
            {
                result = _downloadableAddonsProvider.GetDownloadableAddons(AddonTypeEnum.Map);
            }
            else if (FilterSelectedItem is FilterItemEnum.Mods)
            {
                result = _downloadableAddonsProvider.GetDownloadableAddons(AddonTypeEnum.Mod);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(FilterSelectedItem), FilterSelectedItem, $"Unsupported filter value: {FilterSelectedItem}.");
            }

            if (IsHideInstalledChecked)
            {
                result = result.Where(x => !x.IsInstalled || x.IsUpdateAvailable);
            }

            if (!string.IsNullOrWhiteSpace(SearchBoxText))
            {
                result = result.Where(x => x.Title.Contains(SearchBoxText, StringComparison.OrdinalIgnoreCase));
            }

            return [.. result];
        }
    }

    /// <summary>
    ///     Download/install progress
    /// </summary>
    public float ProgressBarValue { get; set; }

    /// <summary>
    ///     Description of the selected addon
    /// </summary>
    public string SelectedDownloadableDescription => SelectedDownloads.FirstOrDefault()?.ToMarkdownString() ?? string.Empty;

    /// <summary>
    ///     Text of the download button
    /// </summary>
    public string DownloadButtonText
    {
        get
        {
            if (SelectedDownloads is null or [])
            {
                return "Download";
            }
            else
            {
                return $"Download ({SelectedDownloads.Sum(x => x.FileSize).ToSizeString()})";
            }
        }
    }

    /// <summary>
    ///     Gets the list of available filter items.
    /// </summary>
    public List<FilterItemEnum> FilterItems => [.. Enum.GetValues<FilterItemEnum>()];

    /// <summary>
    ///     Gets or sets whether a download is in progress.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelDownloadCommand))]
    private bool _isInProgress;

    /// <summary>
    ///     Currently selected downloadable campaigns, maps or mods
    /// </summary>
    /// <summary>
    ///     Gets or sets the selected downloadable addons.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DownloadableAddonJsonModel> _selectedDownloads = [];

    /// <summary>
    ///     Gets or sets the progress message.
    /// </summary>
    [ObservableProperty]
    private string _progressMessage = string.Empty;

    /// <summary>
    ///     Gets or sets the selected filter item.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadableList))]
    private FilterItemEnum _filterSelectedItem;

    /// <summary>
    ///     Search box text
    /// </summary>
    /// <summary>
    ///     Gets or sets the search box text.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadableList))]
    [NotifyCanExecuteChangedFor(nameof(ClearSearchBoxCommand))]
    private string _searchBoxText = string.Empty;

    /// <summary>
    ///     State of the Hide installed checkbox.
    /// </summary>
    /// <summary>
    ///     Gets or sets whether to hide installed addons.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadableList))]
    private bool _isHideInstalledChecked = false;

    #endregion


    #region Relay Commands

    /// <summary>
    ///     VM initialization
    /// </summary>
    public async Task InitializeAsync()
    {
        await Task.Delay(2000).ConfigureAwait(true);
        await UpdateAsync(false).ConfigureAwait(true);
    }


    /// <summary>
    ///     Update downloadable list
    /// </summary>
    [RelayCommand]
    private async Task UpdateAsync(bool? createNew)
    {
        try
        {
            IsInProgress = true;

            var result = await _downloadableAddonsProvider.CreateCacheAsync(createNew ?? true).ConfigureAwait(true);

            if (!result)
            {
                NotificationsHelper.Show(
                    $"Error while getting downloadable addons for{Environment.NewLine}{Game.FullName}",
                    NotificationType.Error
                    );
            }


            OnPropertyChanged(nameof(DownloadableList));

            SelectedDownloads.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while updating downloadable addons ===");
        }
        finally
        {
            IsInProgress = false;
        }
    }


    /// <summary>
    ///     Download selected addon
    /// </summary>
    [RelayCommand(CanExecute = nameof(DownloadSelectedAddonCanExecute))]
    private async Task DownloadAddon()
    {
        DownloadableAddonJsonModel? _currentDownloadable = null;

        try
        {
            if (SelectedDownloads is null or [])
            {
                return;
            }

            List<DownloadableAddonJsonModel> filesToDownload = [.. SelectedDownloads];

            IsInProgress = true;

            _cancellationTokenSource = new();

            _downloadableAddonsProvider.Progress.ProgressChanged += OnProgressChanged;

            byte downloadCount = 1;

            foreach (var item in filesToDownload)
            {
                _currentDownloadable = item;

                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    throw new OperationCanceledException(_cancellationTokenSource.Token);
                }

                ProgressMessage = $"Downloading {item.Title}. File {downloadCount} of {filesToDownload.Count}.";

                var isDownloaded = await _downloadableAddonsProvider.DownloadAddonAsync(item, _cancellationTokenSource.Token).ConfigureAwait(true);

                if (!isDownloaded)
                {
                    NotificationsHelper.Show(
                        $"Error while downloading {item.Title}.",
                        NotificationType.Error
                        );
                }

                downloadCount++;
            }
        }
        catch (OperationCanceledException)
        {
            //nothing to do
        }
        catch (Exception ex)
        {
            NotificationsHelper.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, $"=== Error while downloading addon {_currentDownloadable?.DownloadUrl} ===");
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _downloadableAddonsProvider.Progress.ProgressChanged -= OnProgressChanged;
            OnProgressChanged(null, 0);
            IsInProgress = false;
            ProgressMessage = string.Empty;
        }
    }

    /// <summary>
    ///     Determines whether the download command can execute.
    /// </summary>
    private bool DownloadSelectedAddonCanExecute => true; /*SelectedDownloadable is not null;*/


    /// <summary>
    ///     Cancel addon download
    /// </summary>
    [RelayCommand(CanExecute = nameof(CancelDownloadCanExecute))]
    private void CancelDownload()
    {
        _cancellationTokenSource?.Cancel();
    }

    /// <summary>
    ///     Determines whether the cancel download command can execute.
    /// </summary>
    private bool CancelDownloadCanExecute => IsInProgress;


    /// <summary>
    ///     Clear search bar
    /// </summary>
    [RelayCommand(CanExecute = nameof(ClearSearchBoxCanExecute))]
    private void ClearSearchBox() => SearchBoxText = string.Empty;

    /// <summary>
    ///     Determines whether the clear search box command can execute.
    /// </summary>
    /// <returns>True if the search box text is not empty.</returns>
    private bool ClearSearchBoxCanExecute() => !string.IsNullOrEmpty(SearchBoxText);

    #endregion
}
