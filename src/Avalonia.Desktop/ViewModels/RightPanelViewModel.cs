using Common.Client.Providers;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Avalonia.Desktop.ViewModels;

public partial class RightPanelViewModel : ObservableObject
{
    public virtual IAddon? SelectedAddon { get; set; }

    private readonly PlaytimeProvider _playtimeProvider;
    private readonly RatingProvider _ratingProvider;


    public RightPanelViewModel(
        PlaytimeProvider playtimeProvider,
        RatingProvider ratingProvider
        )
    {
        _playtimeProvider = playtimeProvider;
        _ratingProvider = ratingProvider;
    }


    #region Binding Properties

    /// <summary>
    /// Description of the selected campaign
    /// </summary>
    public string SelectedAddonDescription => SelectedAddon is null ? string.Empty : SelectedAddon.ToMarkdownString();

    /// <summary>
    /// Preview image of the selected campaign
    /// </summary>
    public Stream? SelectedAddonPreview
    {
        get
        {
            if (SelectedAddon?.Type is AddonTypeEnum.TC)
            {
                return SelectedAddon?.PreviewImage;
            }
            else if (SelectedAddon?.Type is AddonTypeEnum.Map or AddonTypeEnum.Mod)
            {
                return SelectedAddon?.PreviewImage ?? SelectedAddon?.GridImage;
            }

            return null;
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

            var rating = _ratingProvider.GetRating(SelectedAddon.Id);

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

            var time = _playtimeProvider.GetTime(SelectedAddon.Id);

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

        await _ratingProvider.ChangeScoreAsync(SelectedAddon.Id, rating).ConfigureAwait(true);

        OnPropertyChanged(nameof(SelectedAddonRating));
    }

    #endregion        
}
