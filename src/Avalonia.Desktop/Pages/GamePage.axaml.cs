using Avalonia.Controls;

namespace Avalonia.Desktop.Pages;

/// <summary>
///     Displays addons and actions for a selected game.
/// </summary>
public sealed partial class GamePage : UserControl
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GamePage" /> class.
    /// </summary>
    public GamePage()
    {
        InitializeComponent();
    }

    /// <summary> Gets or restores the selected sub-tab, falling back to Campaigns. </summary>
    internal string SelectedSubTab
    {
        get => (GameTabs.SelectedItem as TabItem)?.Name ?? nameof(CampaignsTab);
        set => GameTabs.SelectedItem = GameTabs.Items.OfType<TabItem>().FirstOrDefault(tab =>
            tab.Name == value && tab.IsEnabled && tab.IsVisible) ?? CampaignsTab;
    }
}
