using System.Collections.ObjectModel;
using System.ComponentModel;
using Addons.Addons;
using Avalonia.Desktop.Misc;
using Avalonia.Media.Imaging;
using Common.All.Helpers;
using Common.Client.Config;
using Common.Client.Interfaces;
using Common.Client.Providers;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Avalonia.Desktop.ViewModels;

public partial class RightPanelViewModel : ObservableObject
{
    public virtual BaseAddon? SelectedAddon { get; set; }

    private readonly PlaytimeProvider _playtimeProvider;
    private readonly RatingProvider _ratingProvider;
    private readonly BitmapsCache _bitmapsCache;
    private readonly IConfigProvider _config;


    public RightPanelViewModel(
        PlaytimeProvider playtimeProvider,
        RatingProvider ratingProvider,
        BitmapsCache bitmapsCache,
        IConfigProvider config
        )
    {
        _playtimeProvider = playtimeProvider;
        _ratingProvider = ratingProvider;
        _bitmapsCache = bitmapsCache;
        _config = config;
    }


    #region Binding Properties

    /// <summary>
    /// Description of the selected campaign
    /// </summary>
    public string SelectedAddonDescription => SelectedAddon is null ? string.Empty : SelectedAddon.ToMarkdownString();

    /// <summary>
    /// Preview image of the selected campaign
    /// </summary>
    public Bitmap? SelectedAddonPreview => SelectedAddon?.PreviewImageHash is null ? null : _bitmapsCache.GetFromCache(SelectedAddon.PreviewImageHash.Value);

    /// <summary>
    /// Is preview image in the description visible
    /// </summary>
    public bool IsPreviewVisible => SelectedAddonPreview is not null;

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

    public bool HasOptions => SelectedAddon?.Options is not null;

    public ObservableCollection<AddonOption> AddonOptions { get; } = new();

    #endregion


    #region Relay Commands

    /// <summary>
    /// Upvote fix
    /// </summary>
    [RelayCommand]
    private async Task ChangeRatingAsync(string score)
    {
        Guard.IsNotNull(SelectedAddon);

        var rating = byte.Parse(score);

        await _ratingProvider.ChangeScoreAsync(SelectedAddon.AddonId.Id, rating).ConfigureAwait(true);

        OnPropertyChanged(nameof(SelectedAddonRating));
    }

    #endregion      


    protected void UpdateAddonOptions()
    {
        foreach (var addon in AddonOptions)
        {
            addon.PropertyChanged -= OnPropertyChanged;
        }

        AddonOptions.Clear();

        if (SelectedAddon?.Options is null)
        {
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

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName?.Equals(nameof(AddonOption.IsEnabled)) is true &&
            sender is AddonOption option)
        {
            ArgumentNullException.ThrowIfNull(SelectedAddon);

            _config.ChangeAddonOptionState(SelectedAddon.AddonId.Id, option.Name, option.IsEnabled);
        }
    }

    public sealed partial class AddonOption : ObservableObject
    {
        public string Name { get; }

        [ObservableProperty]
        private bool _isEnabled;

        public AddonOption(string name, bool isEnabled)
        {
            Name = name;
            IsEnabled = isEnabled;
        }
    }
}
