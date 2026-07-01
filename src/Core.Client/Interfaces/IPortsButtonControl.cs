using CommunityToolkit.Mvvm.Input;

namespace Core.Client.Interfaces;

/// <summary>
///     Defines the interface for port button controls used in the ports view.
/// </summary>
public interface IPortsButtonControl
{
    /// <summary>
    ///     Gets the command to open the port installation folder.
    /// </summary>
    IRelayCommand? OpenFolderCommand { get; }

    /// <summary>
    ///     Gets the command to refresh the ports list.
    /// </summary>
    IAsyncRelayCommand? RefreshListCommand { get; }

    /// <summary>
    ///     Gets whether the port buttons are visible.
    /// </summary>
    bool IsPortsButtonsVisible { get; }

    /// <summary>
    ///     Gets or sets whether a port operation is currently in progress.
    /// </summary>
    bool IsInProgress { get; set; }
}
