using CommunityToolkit.Mvvm.Input;

namespace Common.Interfaces;

public interface IPortsButtonControl
{
    IRelayCommand? OpenFolderCommand { get; }

    IAsyncRelayCommand? RefreshListCommand { get; }

    bool IsPortsButtonsVisible { get; }

    bool IsInProgress { get; set; }
}
