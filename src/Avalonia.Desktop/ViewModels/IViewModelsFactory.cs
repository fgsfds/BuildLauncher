using Core.All.Enums;
using Ports.Ports;
using Tools.Tools;

namespace Avalonia.Desktop.ViewModels;

/// <summary>
///     Factory for creating application view models.
/// </summary>
public interface IViewModelsFactory
{
    /// <summary>
    ///     Gets the main window view model.
    /// </summary>
    /// <returns>
    ///     The main window view model.
    /// </returns>
    MainWindowViewModel GetMainWindowViewModel();

    /// <summary>
    ///     Gets the campaigns view model for the specified game.
    /// </summary>
    /// <param name="gameEnum">
    ///     The game.
    /// </param>
    /// <returns>
    ///     The campaigns view model.
    /// </returns>
    CampaignsViewModel GetCampaignsViewModel(GameEnum gameEnum);

    /// <summary>
    ///     Gets the maps view model for the specified game.
    /// </summary>
    /// <param name="gameEnum">
    ///     The game.
    /// </param>
    /// <returns>
    ///     The maps view model.
    /// </returns>
    MapsViewModel GetMapsViewModel(GameEnum gameEnum);

    /// <summary>
    ///     Gets the mods view model for the specified game.
    /// </summary>
    /// <param name="gameEnum">
    ///     The game.
    /// </param>
    /// <returns>
    ///     The mods view model.
    /// </returns>
    ModsViewModel GetModsViewModel(GameEnum gameEnum);

    /// <summary>
    ///     Gets the downloads view model for the specified game.
    /// </summary>
    /// <param name="gameEnum">
    ///     The game.
    /// </param>
    /// <returns>
    ///     The downloads view model.
    /// </returns>
    DownloadsViewModel GetDownloadsViewModel(GameEnum gameEnum);

    /// <summary>
    ///     Gets the view model for the specified port.
    /// </summary>
    /// <param name="port">
    ///     The port.
    /// </param>
    /// <returns>
    ///     The port view model.
    /// </returns>
    PortViewModel GetPortViewModel(BasePort port);

    /// <summary>
    ///     Gets the view model for the specified tool.
    /// </summary>
    /// <param name="tool">
    ///     The tool.
    /// </param>
    /// <returns>
    ///     The tool view model.
    /// </returns>
    ToolViewModel GetToolViewModel(BaseTool tool);
}
