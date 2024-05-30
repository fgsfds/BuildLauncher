using ClientCommon.API;
using ClientCommon.Config;
using ClientCommon.Providers;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BuildLauncher.ViewModels;

public partial class RightPanelViewModel : ObservableObject, IRightPanelControl
{
    public virtual IAddon? SelectedAddon { get; set; }

    private readonly ConfigProvider _config;
    private readonly PlaytimeProvider _playtimeProvider;
    private readonly ApiInterface _apiInterface;
    private readonly ScoresProvider _scoresProvider;


    public RightPanelViewModel(
        ConfigProvider config,
        PlaytimeProvider playtimeProvider,
        ApiInterface apiInterface,
        ScoresProvider scoresProvider
        )
    {
        _config = config;
        _playtimeProvider = playtimeProvider;
        _apiInterface = apiInterface;
        _scoresProvider = scoresProvider;
    }


    #region Binding Properties

    /// <summary>
    /// Description of the selected campaign
    /// </summary>
    public string SelectedAddonDescription => SelectedAddon is null ? string.Empty : SelectedAddon.ToMarkdownString();

    /// <summary>
    /// Preview image of the selected campaign
    /// </summary>
    public Stream? SelectedAddonPreview => SelectedAddon?.PreviewImage;

    /// <summary>
    /// Is preview image in the description visible
    /// </summary>
    public bool IsPreviewVisible => SelectedAddon?.PreviewImage is not null;

    public int? SelectedAddonScore
    {
        get
        {
            if (SelectedAddon is null)
            {
                return null;
            }

            var hasUpvote = _scoresProvider.GetScore(SelectedAddon.Id);

            if (hasUpvote is not null)
            {
                return hasUpvote;
            }

            return null;
        }
    }

    public bool IsSelectedAddonUpvoted
    {
        get
        {
            if (SelectedAddon is null)
            {
                return false;
            }

            var hasUpvote = _config.Scores.TryGetValue(SelectedAddon.Id, out var isUpvote);

            if (hasUpvote && isUpvote)
            {
                return true;
            }

            return false;
        }
    }

    public bool IsSelectedAddonDownvoted
    {
        get
        {
            if (SelectedAddon is null)
            {
                return false;
            }

            var hasUpvote = _config.Scores.TryGetValue(SelectedAddon.Id, out var isUpvote);

            if (hasUpvote && !isUpvote)
            {
                return true;
            }

            return false;
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
    private async Task Upvote()
    {
        SelectedAddon.ThrowIfNull();

        await _scoresProvider.ChangeScoreAsync(SelectedAddon.Id, true).ConfigureAwait(true);

        OnPropertyChanged(nameof(SelectedAddonScore));
        OnPropertyChanged(nameof(IsSelectedAddonUpvoted));
        OnPropertyChanged(nameof(IsSelectedAddonDownvoted));
    }


    /// <summary>
    /// Downvote fix
    /// </summary>
    [RelayCommand]
    private async Task Downvote()
    {
        SelectedAddon.ThrowIfNull();

        await _scoresProvider.ChangeScoreAsync(SelectedAddon.Id, false).ConfigureAwait(true);

        OnPropertyChanged(nameof(SelectedAddonScore));
        OnPropertyChanged(nameof(IsSelectedAddonUpvoted));
        OnPropertyChanged(nameof(IsSelectedAddonDownvoted));
    }

    #endregion        
}
