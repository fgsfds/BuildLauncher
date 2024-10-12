using CommunityToolkit.Mvvm.Input;

namespace Common.Client.Interfaces;

public interface IRightPanelControl
{
    bool IsPreviewVisible { get; }
    Stream? SelectedAddonPreview { get; }
    string? SelectedAddonDescription { get; }
    string? SelectedAddonPlaytime { get; }
    string? SelectedAddonRating { get; }
    IAsyncRelayCommand<string> ChangeRatingCommand { get; }
}
