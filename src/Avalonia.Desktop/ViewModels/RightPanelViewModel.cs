using System.Collections.ObjectModel;
using System.ComponentModel;
using Addons.Addons;
using Addons.Providers;
using Avalonia.Desktop.Misc;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.All.Helpers;
using Core.Client.Interfaces;
using Core.Client.Providers;

namespace Avalonia.Desktop.ViewModels;

public abstract partial class RightPanelViewModel : ObservableObject
{
    private readonly BitmapsCache _bitmapsCache;
    private readonly IConfigProvider _config;
    private readonly MetadataProvider _metadataProvider;
    private readonly PlaytimeProvider _playtimeProvider;
    private readonly RatingProvider _ratingProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RightPanelViewModel" /> class.
    /// </summary>
    /// <param name="playtimeProvider">
    ///     The playtime provider.
    /// </param>
    /// <param name="ratingProvider">
    ///     The rating provider.
    /// </param>
    /// <param name="metadataProvider">
    ///     The metadata provider.
    /// </param>
    /// <param name="bitmapsCache">
    ///     The bitmaps cache.
    /// </param>
    /// <param name="config">
    ///     The configuration provider.
    /// </param>
    protected RightPanelViewModel(
        PlaytimeProvider playtimeProvider,
        RatingProvider ratingProvider,
        MetadataProvider metadataProvider,
        BitmapsCache bitmapsCache,
        IConfigProvider config
        )
    {
        _playtimeProvider = playtimeProvider;
        _ratingProvider = ratingProvider;
        _metadataProvider = metadataProvider;
        _bitmapsCache = bitmapsCache;
        _config = config;
    }

    /// <summary>
    ///     Gets or sets the selected addon.
    /// </summary>
    public virtual BaseAddon? SelectedAddon { get; set; }


    /// <summary>
    ///     Updates the addon options from the selected addon.
    /// </summary>
    protected void UpdateAddonOptions()
    {
        foreach (var addon in AddonOptions)
        {
            addon.PropertyChanged -= OnPropertyChanged;
        }

        AddonOptions.Clear();

        if (SelectedAddon?.Options is null)
        {
            OnPropertyChanged(nameof(HasOptions));

            return;
        }

        var enabled = _config.GetEnabledOptions(SelectedAddon.AddonId.Id);

        foreach (var option in SelectedAddon.Options)
        {
            AddonOptions.Add(new(option.Key, enabled.Contains(option.Key)));
        }

        foreach (var addon in AddonOptions)
        {
            addon.PropertyChanged += OnPropertyChanged;
        }

        OnPropertyChanged(nameof(HasOptions));
    }

    /// <summary>
    ///     Handles the property changed event for addon options.
    /// </summary>
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName?.Equals(nameof(AddonOption.IsEnabled)) is not true ||
            sender is not AddonOption option)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(SelectedAddon);

        _config.ChangeAddonOptionState(SelectedAddon.AddonId.Id, option.Name, option.IsEnabled);
    }


    /// <summary>
    ///     Represents a toggleable addon option.
    /// </summary>
    public sealed partial class AddonOption : ObservableObject
    {
        /// <summary>
        ///     Gets or sets whether the option is enabled.
        /// </summary>
        [ObservableProperty]
        private bool _isEnabled;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AddonOption" /> class.
        /// </summary>
        /// <param name="name">
        ///     The option name.
        /// </param>
        /// <param name="isEnabled">
        ///     Whether the option is enabled.
        /// </param>
        public AddonOption(string name, bool isEnabled)
        {
            Name = name;
            IsEnabled = isEnabled;
        }

        /// <summary>
        ///     Gets the option name.
        /// </summary>
        public string Name { get; }
    }


    #region Binding Properties

    /// <summary>
    ///     Description of the selected campaign
    /// </summary>
    public string SelectedAddonDescription => SelectedAddon is null ? string.Empty : SelectedAddon.ToMarkdownString();

    /// <summary>
    ///     Preview image of the selected campaign
    /// </summary>
    public Bitmap? SelectedAddonPreview => SelectedAddon?.PreviewImageHash is null ? null : _bitmapsCache.GetFromCache(SelectedAddon.PreviewImageHash.Value);

    /// <summary>
    ///     Is preview image in the description visible
    /// </summary>
    public bool IsPreviewVisible => SelectedAddonPreview is not null;

    /// <summary>
    ///     Gets whether a metadata update is available for the selected addon.
    /// </summary>
    public bool IsMetadataUpdateAvailable => SelectedAddon?.FileInfo is not null && _metadataProvider.IsMetadataUpdateAvailable(SelectedAddon.AddonId, SelectedAddon.FileInfo);

    /// <summary>
    ///     Gets the rating of the selected addon.
    /// </summary>
    public string? SelectedAddonRating
    {
        get
        {
            if (SelectedAddon is null)
            {
                return null;
            }

            var rating = _ratingProvider.GetRating(SelectedAddon.AddonId.Id);

            if (rating is null)
            {
                return null;
            }

            if (rating == 0)
            {
                return "-";
            }

            return rating.Value.ToString("0.##");
        }
    }


    /// <summary>
    ///     Gets the playtime of the selected addon.
    /// </summary>
    public string? SelectedAddonPlaytime
    {
        get
        {
            if (SelectedAddon is null)
            {
                return null;
            }

            var time = _playtimeProvider.GetTime(SelectedAddon.AddonId.Id);

            if (time is not null)
            {
                return $"Play time: {time.Value.ToTimeString()}";
            }

            return "Never played";
        }
    }

    /// <summary>
    ///     Gets whether the selected addon has configurable options.
    /// </summary>
    public bool HasOptions => SelectedAddon?.Options?.Count > 0;

    /// <summary>
    ///     Gets the list of addon options.
    /// </summary>
    public ObservableCollection<AddonOption> AddonOptions { get; } = new();

    #endregion


    #region Relay Commands

    /// <summary>
    ///     Upvote fix
    /// </summary>
    [RelayCommand]
    private async Task ChangeRatingAsync(string score)
    {
        ArgumentNullException.ThrowIfNull(SelectedAddon);

        var rating = byte.Parse(score);

        await _ratingProvider.ChangeScoreAsync(SelectedAddon.AddonId.Id, rating).ConfigureAwait(true);

        OnPropertyChanged(nameof(SelectedAddonRating));
    }

    /// <summary>
    ///     Updates metadata for the specified addon.
    /// </summary>
    /// <param name="value">
    ///     The addon to update, or null to use the selected addon.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation.
    /// </returns>
    public abstract Task UpdateMetadataAsync(object? value);

    #endregion
}
