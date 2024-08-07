using Common.Entities;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mods.Providers;
using System.Collections.Immutable;

namespace BuildLauncher.ViewModels;

public sealed partial class DownloadsViewModel : ObservableObject
{
    public readonly IGame Game;

    public readonly InstalledAddonsProvider _installedAddonsProvider;
    public readonly DownloadableAddonsProvider _downloadableAddonsProvider;

    [ObservableProperty]
    private bool _isInProgress;


    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public DownloadsViewModel(
        IGame game,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory downloadableAddonsProviderFactory
        )
    {
        Game = game;

        _installedAddonsProvider = installedAddonsProviderFactory.GetSingleton(game);
        _downloadableAddonsProvider = downloadableAddonsProviderFactory.GetSingleton(game);

        _installedAddonsProvider.AddonsChangedEvent += OnAddonChanged;
        _downloadableAddonsProvider.AddonDownloadedEvent += OnAddonChanged;
    }


    #region Binding Properties

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
                ThrowHelper.ArgumentOutOfRangeException(nameof(FilterSelectedItem));
                return null;
            }

            if (string.IsNullOrWhiteSpace(SearchBoxText))
            {
                return [.. result];
            }
            else
            {
                return [.. result.Where(x => x.Title.Contains(SearchBoxText, StringComparison.OrdinalIgnoreCase))];
            }
        }
    }

    /// <summary>
    /// Download/install progress
    /// </summary>
    public float ProgressBarValue { get; set; }

    /// <summary>
    /// Currently selected downloadable campaign, map or mod
    /// </summary>
    private DownloadableAddonEntity? _selectedDownloadable;
    public DownloadableAddonEntity? SelectedDownloadable
    {
        get => _selectedDownloadable;
        set
        {
            _selectedDownloadable = value;
            OnPropertyChanged(nameof(SelectedDownloadable));
            OnPropertyChanged(nameof(SelectedDownloadableDescription));
            OnPropertyChanged(nameof(DownloadButtonText));

            DownloadAddonCommand.NotifyCanExecuteChanged();
        }
    }

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


    #region Relay Commands

    /// <summary>
    /// VM initialization
    /// </summary>
    public Task InitializeAsync() => UpdateAsync();

    /// <summary>
    /// Update downloadable addons list
    /// </summary>
    private async Task UpdateAsync()
    {
        await _downloadableAddonsProvider.CreateCacheAsync(false).ConfigureAwait(true);

        OnPropertyChanged(nameof(DownloadableList));
    }


    /// <summary>
    /// Update downloadables list
    /// </summary>
    [RelayCommand]
    private async Task Update()
    {
        IsInProgress = true;

        await _downloadableAddonsProvider.CreateCacheAsync(true).ConfigureAwait(true);

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
        if (SelectedDownloadable is null)
        {
            return;
        }

        _downloadableAddonsProvider.Progress.ProgressChanged += OnProgressChanged;

        await _downloadableAddonsProvider.DownloadAddonAsync(SelectedDownloadable).ConfigureAwait(true);

        _downloadableAddonsProvider.Progress.ProgressChanged -= OnProgressChanged;
        OnProgressChanged(null, 0);
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
