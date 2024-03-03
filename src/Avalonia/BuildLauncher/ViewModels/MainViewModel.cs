using Common.Config;
using CommunityToolkit.Mvvm.ComponentModel;
using Games.Providers;

namespace BuildLauncher.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly ConfigEntity _config;
    private readonly GamesProvider _gamesProvider;

    public MainViewModel(
        ConfigProvider config,
        GamesProvider gamesProvider)
    {
        _config = config.Config;
        _gamesProvider = gamesProvider;

        _config.NotifyParameterChanged += NotifyParameterChanged;
    }


    #region Binding Properties

    /// <summary>
    /// Is Blood tab enabled
    /// </summary>
    public bool IsBloodTabEnabled => _gamesProvider.Blood.IsBaseGameInstalled;

    /// <summary>
    /// Is Duke Nukem 3D tab enabled
    /// </summary>
    public bool IsDukeTabEnabled => _gamesProvider.Duke3D.IsBaseGameInstalled || _gamesProvider.Duke3D.IsDuke64Installed || _gamesProvider.Duke3D.IsWorldTourInstalled;

    /// <summary>
    /// Is Shadow Warrior tab enabled
    /// </summary>
    public bool IsWangTabEnabled => _gamesProvider.Wang.IsBaseGameInstalled;

    #endregion


    /// <summary>
    /// Update VM with path to the game changes in the config
    /// </summary>
    /// <param name="parameterName">Config parameter</param>
    private void NotifyParameterChanged(string parameterName)
    {
        if (parameterName.Equals(nameof(_config.GamePathBlood)))
        {
            OnPropertyChanged(nameof(IsBloodTabEnabled));
        }
        else if (parameterName.Equals(nameof(_config.GamePathDuke3D)) ||
                 parameterName.Equals(nameof(_config.GamePathDukeWT)) ||
                 parameterName.Equals(nameof(_config.GamePathDuke64)))
        {
            OnPropertyChanged(nameof(IsDukeTabEnabled));
        }
        else if (parameterName.Equals(nameof(_config.GamePathWang)))
        {
            OnPropertyChanged(nameof(IsWangTabEnabled));
        }
    }
}
