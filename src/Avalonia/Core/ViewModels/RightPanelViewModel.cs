using Common.Client.API;
using Common.Client.Config;
using Common.Client.Providers;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BuildLauncher.ViewModels;

public partial class RightPanelViewModel : ObservableObject, IRightPanelControl
{
    public virtual IAddon? SelectedAddon { get; set; }

    private readonly IConfigProvider _config;
    private readonly PlaytimeProvider _playtimeProvider;
    private readonly ApiInterface _apiInterface;
    private readonly RatingProvider _ratingProvider;


    public RightPanelViewModel(
        IConfigProvider config,
        PlaytimeProvider playtimeProvider,
        ApiInterface apiInterface,
        RatingProvider ratingProvider
        )
    {
        _config = config;
        _playtimeProvider = playtimeProvider;
        _apiInterface = apiInterface;
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
            if (SelectedAddon is null)
            {
                return null;
            }

            if (SelectedAddon.Type is AddonTypeEnum.TC)
            {
                return SelectedAddon?.PreviewImage;
            }
            else if (SelectedAddon.Type is AddonTypeEnum.Map)
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
        SelectedAddon.ThrowIfNull();

        var rating = byte.Parse(score);

        await _ratingProvider.ChangeScoreAsync(SelectedAddon.Id, rating).ConfigureAwait(true);

        OnPropertyChanged(nameof(SelectedAddonRating));
    }

    #endregion        
}
