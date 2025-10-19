using Avalonia.Desktop.Misc;
using Avalonia.Media.Imaging;
using Common.All.Helpers;
using Common.All.Interfaces;
using Common.Client.Providers;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Avalonia.Desktop.ViewModels;

public partial class RightPanelViewModel : ObservableObject
{
    public virtual IAddon? SelectedAddon { get; set; }

    private readonly PlaytimeProvider _playtimeProvider;
    private readonly RatingProvider _ratingProvider;
    private readonly BitmapsCache _bitmapsCache;


    public RightPanelViewModel(
        PlaytimeProvider playtimeProvider,
        RatingProvider ratingProvider,
        BitmapsCache bitmapsCache
        )
    {
        _playtimeProvider = playtimeProvider;
        _ratingProvider = ratingProvider;
        _bitmapsCache = bitmapsCache;
    }


    #region Binding Properties

    /// <summary>
    /// Description of the selected campaign
    /// </summary>
    public string SelectedAddonDescription => SelectedAddon is null ? string.Empty : SelectedAddon.ToMarkdownString();

    /// <summary>
    /// Preview image of the selected campaign
    /// </summary>
    public Bitmap? SelectedAddonPreview
    {
        get
        {
            return SelectedAddon?.PreviewImageHash is null ? null : _bitmapsCache.GetFromCache(SelectedAddon.PreviewImageHash.Value);
        }
    }

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
}
