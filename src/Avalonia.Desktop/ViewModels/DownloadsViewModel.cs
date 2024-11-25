using Addons.Providers;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Common.Entities;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class DownloadsViewModel : ObservableObject
{
    public readonly IGame Game;

    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly DownloadableAddonsProvider _downloadableAddonsProvider;

    private CancellationTokenSource? _cancellationTokenSource;
    private readonly ILogger _logger;


    #region Binding Properties

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelDownloadCommand))]
    private bool _isInProgress;

    [ObservableProperty]
    private bool _hasUpdates;

    /// <summary>
    /// List of downloadable addons
    /// </summary>
    public ImmutableList<IDownloadableAddon> DownloadableList
    {
        get
        {
            IEnumerable<IDownloadableAddon> result;

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
    /// Currently selected downloadable campaign, map or mod
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedDownloadableDescription))]
    [NotifyPropertyChangedFor(nameof(DownloadButtonText))]
    [NotifyCanExecuteChangedFor(nameof(DownloadAddonCommand))]
    private DownloadableAddonEntity? _selectedDownloadable;

    /// <summary>
    /// Currently selected downloadable campaign, map or mod
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonText))]
    private List<DownloadableAddonEntity>? _selectedDownloadableList;

    [ObservableProperty]
    private string _progressMessage = string.Empty;

    /// <summary>
    /// Description of the selected addon
    /// </summary>
    public string SelectedDownloadableDescription => SelectedDownloadable is null ? string.Empty : SelectedDownloadable.ToMarkdownString();

    /// <summary>
    /// Text of the download button
    /// </summary>
    public string DownloadButtonText
    {
        get
        {
            if (SelectedDownloadableList is null or [])
            {
                return "Download";
            }
            else
            {
                return $"Download ({SelectedDownloadableList.Sum(x => x.FileSize).ToSizeString()})";
            }
        }
    }

    public List<FilterItemEnum> FilterItems => [.. Enum.GetValues<FilterItemEnum>()];

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
        IGame game,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory downloadableAddonsProviderFactory,
        ILogger logger
        )
    {
        Game = game;

        _installedAddonsProvider = installedAddonsProviderFactory.GetSingleton(game);
        _downloadableAddonsProvider = downloadableAddonsProviderFactory.GetSingleton(game);
        _logger = logger;

        _installedAddonsProvider.AddonsChangedEvent += OnAddonChanged;
        _downloadableAddonsProvider.AddonDownloadedEvent += OnAddonChanged;
    }


    #region Relay Commands

    /// <summary>
    /// VM initialization
    /// </summary>
    public async Task InitializeAsync()
    {
        await Task.Delay(2000).ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(
            async () => await UpdateAsync(false).ConfigureAwait(false)
            ).ConfigureAwait(false);
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
                var length = App.Random.Next(1, 100);
                var repeatedString = new string('\u200B', length);

                App.NotificationManager.Show(
                    $"Error while getting downloadable addons for{Environment.NewLine}{Game.FullName}" + repeatedString,
                    NotificationType.Error
                    );
            }


            OnPropertyChanged(nameof(DownloadableList));

            SelectedDownloadable = null;
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
        try
        {
            if (SelectedDownloadableList is null or [])
            {
                return;
            }

            List<DownloadableAddonEntity> filesToDownload = [.. SelectedDownloadableList];

            IsInProgress = true;

            _cancellationTokenSource = new();

            _downloadableAddonsProvider.Progress.ProgressChanged += OnProgressChanged;

            byte downloadCount = 1;

            foreach (var item in filesToDownload)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    ThrowHelper.ThrowOperationCanceledException();
                }

                ProgressMessage = $"Downloading {item.Title}. File {downloadCount} of {filesToDownload.Count}.";

                await _downloadableAddonsProvider.DownloadAddonAsync(item, _cancellationTokenSource.Token).ConfigureAwait(true);

                downloadCount++;
            }
        }
        catch (OperationCanceledException)
        {
            //nothing to do
        }
        catch (Exception ex)
        {
            var length = App.Random.Next(1, 100);
            var repeatedString = new string('\u200B', length);

            App.NotificationManager.Show(
                "Critical error! Exception is written to the log." + repeatedString,
                NotificationType.Error
                );

            _logger.LogCritical(ex, $"=== Error while downloading addon {SelectedDownloadable?.DownloadUrl} ===");
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
    private bool DownloadSelectedAddonCanExecute => SelectedDownloadable is not null;


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

    private void OnAddonChanged(IGame game, AddonTypeEnum? addonType)
    {
        if (game.GameEnum != Game.GameEnum)
        {
            return;
        }

        OnPropertyChanged(nameof(DownloadableList));
    }

    public enum FilterItemEnum
    {
        All,
        TCs,
        Maps,
        Mods
    }
}
