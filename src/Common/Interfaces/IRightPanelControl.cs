using CommunityToolkit.Mvvm.Input;

namespace Common.Interfaces
{
    public interface IRightPanelControl
    {
        bool IsPreviewVisible { get; }
        Stream? SelectedAddonPreview { get; }
        string? SelectedAddonDescription { get; }
        string? SelectedAddonPlaytime { get; }
        int? SelectedAddonScore { get; }
        bool IsSelectedAddonUpvoted { get; }
        bool IsSelectedAddonDownvoted { get; }
        IAsyncRelayCommand UpvoteCommand { get; }
        IAsyncRelayCommand DownvoteCommand { get; }
    }
}
