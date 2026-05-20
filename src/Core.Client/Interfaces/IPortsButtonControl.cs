using CommunityToolkit.Mvvm.Input;

namespace Core.Client.Interfaces;

public interface IPortsButtonControl
{
    IRelayCommand? OpenFolderCommand { get; }

    IAsyncRelayCommand? RefreshListCommand { get; }

    bool IsPortsButtonsVisible { get; }

    bool IsInProgress { get; set; }
}
