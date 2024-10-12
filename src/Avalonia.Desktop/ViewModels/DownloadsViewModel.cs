using Addons.Providers;
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

    public readonly InstalledAddonsProvider _installedAddonsProvider;
    public readonly DownloadableAddonsProvider _downloadableAddonsProvider;
    private readonly ILogger _logger;


    #region Binding Properties

    [ObservableProperty]
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

            if (string.IsNullOrWhiteSpace(SearchBoxText))
            {
            }
            else
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
    /// Description of the selected addom
    /// </summary>
    public string SelectedDownloadableDescription => SelectedDownloadable is null ? string.Empty : SelectedDownloadable.ToMarkdownString();

    /// <summary>
    /// Text of the download button
    /// </summary>
    public string DownloadButtonText
    {
        get
        {
            if (SelectedDownloadable is null)
            {
                return "Download";
            }
            else
            {
                return $"Download ({SelectedDownloadable.FileSize.ToSizeString()})";
            }
        }
    }

    public List<FilterItemEnum> FilterItems => Enum.GetValues(typeof(FilterItemEnum)).Cast<FilterItemEnum>().ToList();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadableList))]
    private FilterItemEnum _filterSelectedItem;

    /// <summary>
    /// Search box text
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadableList))]
    [NotifyCanExecuteChangedFor(nameof(ClearSearchBoxCommand))]
    private string _searchBoxText;

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
    public Task InitializeAsync()
    {
        Thread.Sleep(2000);
        return UpdateAsync(false);
    }


    /// <summary>
    /// Update downloadables list
    /// </summary>
    [RelayCommand]
    private async Task UpdateAsync(bool? createNew)
    {
        IsInProgress = true;

        await _downloadableAddonsProvider.CreateCacheAsync(createNew ?? true).ConfigureAwait(true);

        OnPropertyChanged(nameof(DownloadableList));

        SelectedDownloadable = null;

        IsInProgress = false;
    }


    /// <summary>
    /// Download selected addon
    /// </summary>
    [RelayCommand(CanExecute = (nameof(DownloadSelectedAddonCanExecute)))]
    private async Task DownloadAddon()
    {
        try
        {
            if (SelectedDownloadable is null)
            {
                return;
            }

            _downloadableAddonsProvider.Progress.ProgressChanged += OnProgressChanged;

            await _downloadableAddonsProvider.DownloadAddonAsync(SelectedDownloadable).ConfigureAwait(true);

            _downloadableAddonsProvider.Progress.ProgressChanged -= OnProgressChanged;
            OnProgressChanged(null, 0);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Critical error ===");
        }
    }
    private bool DownloadSelectedAddonCanExecute => SelectedDownloadable is not null;


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
