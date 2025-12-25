using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Addons.Providers;
using Avalonia.Controls.Notifications;
using Avalonia.Desktop.Misc;
using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Serializable.Downloadable;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Games.Games;
using Microsoft.Extensions.Logging;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class DownloadsViewModel : ObservableObject
{
    public readonly BaseGame Game;

    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly DownloadableAddonsProvider _downloadableAddonsProvider;

    private CancellationTokenSource? _cancellationTokenSource;
    private readonly ILogger _logger;


    #region Binding Properties

    /// <summary>
    /// List of downloadable addons
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
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(FilterSelectedItem));
                return null;
            }

            if (!string.IsNullOrWhiteSpace(SearchBoxText))
            {
                result = result.Where(x => x.Title.Contains(SearchBoxText, StringComparison.OrdinalIgnoreCase));
            }

            HasUpdates = result.Any(x => x.HasNewerVersion);

            return [.. result];
        }
    }

    /// <summary>
    /// Download/install progress
    /// </summary>
    public float ProgressBarValue { get; set; }

    /// <summary>
    /// Description of the selected addon
    /// </summary>
    public string SelectedDownloadableDescription => SelectedDownloads.FirstOrDefault()?.ToMarkdownString() ?? string.Empty;

    /// <summary>
    /// Text of the download button
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

    public List<FilterItemEnum> FilterItems => [.. Enum.GetValues<FilterItemEnum>()];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelDownloadCommand))]
    private bool _isInProgress;

    [ObservableProperty]
    private bool _hasUpdates;

    /// <summary>
    /// Currently selected downloadable campaigns, maps or mods
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DownloadableAddonJsonModel> _selectedDownloads = [];

    [ObservableProperty]
    private string _progressMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadableList))]
    private FilterItemEnum _filterSelectedItem;

    /// <summary>
    /// Search box text
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadableList))]
    [NotifyCanExecuteChangedFor(nameof(ClearSearchBoxCommand))]
    private string _searchBoxText = string.Empty;

    #endregion


    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public DownloadsViewModel(
        BaseGame game,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory downloadableAddonsProviderFactory,
        ILogger logger
        )
    {
        Game = game;

        _installedAddonsProvider = installedAddonsProviderFactory.Get(game);
        _downloadableAddonsProvider = downloadableAddonsProviderFactory.Get(game);
        _logger = logger;

        _installedAddonsProvider.AddonsChangedEvent += OnAddonChanged;
        _downloadableAddonsProvider.AddonDownloadedEvent += OnAddonChanged;
        SelectedDownloads.CollectionChanged += OnSelectedDownloadsChanged;
    }


    #region Relay Commands

    /// <summary>
    /// VM initialization
    /// </summary>
    public async Task InitializeAsync()
    {
        await Task.Delay(2000).ConfigureAwait(true);
        await UpdateAsync(false).ConfigureAwait(true);
    }


    /// <summary>
    /// Update downloadable list
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
    /// Download selected addon
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
                    ThrowHelper.ThrowOperationCanceledException();
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
    private bool DownloadSelectedAddonCanExecute => true; /*SelectedDownloadable is not null;*/


    /// <summary>
    /// Cancel addon download
    /// </summary>
    [RelayCommand(CanExecute = nameof(CancelDownloadCanExecute))]
    private void CancelDownload()
    {
        _cancellationTokenSource?.Cancel();
    }
    private bool CancelDownloadCanExecute => IsInProgress;


    /// <summary>
    /// Clear search bar
    /// </summary>
    [RelayCommand(CanExecute = nameof(ClearSearchBoxCanExecute))]
    private void ClearSearchBox() => SearchBoxText = string.Empty;
    private bool ClearSearchBoxCanExecute() => !string.IsNullOrEmpty(SearchBoxText);

    #endregion


    private void OnProgressChanged(object? sender, float e)
    {
        ProgressBarValue = e;
        OnPropertyChanged(nameof(ProgressBarValue));
    }

    private void OnAddonChanged(GameEnum game, AddonTypeEnum addonType)
    {
        if (game != Game.GameEnum)
        {
            return;
        }

        OnPropertyChanged(nameof(DownloadableList));
    }

    private void OnSelectedDownloadsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SelectedDownloadableDescription));
        OnPropertyChanged(nameof(DownloadButtonText));
        DownloadAddonCommand.NotifyCanExecuteChanged();
    }

    public enum FilterItemEnum
    {
        All,
        TCs,
        Maps,
        Mods
    }
}
