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

    /// <summary>
    /// Is Ion Fury tab enabled
    /// </summary>
    public bool IsFuryTabEnabled => _gamesProvider.Fury.IsBaseGameInstalled;

    /// <summary>
    /// Is Redneck Rampage tab enabled
    /// </summary>
    public bool IsRedneckTabEnabled => _gamesProvider.Redneck.IsBaseGameInstalled || _gamesProvider.Redneck.IsAgainInstalled;

    /// <summary>
    /// Is Powerslave tab enabled
    /// </summary>
    public bool IsSlaveTabEnabled => _gamesProvider.Slave.IsBaseGameInstalled;

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
        else if (parameterName.Equals(nameof(_config.GamePathFury)))
        {
            OnPropertyChanged(nameof(IsFuryTabEnabled));
        }
        else if (parameterName.Equals(nameof(_config.GamePathRedneck)) ||
                 parameterName.Equals(nameof(_config.GamePathAgain)))
        {
            OnPropertyChanged(nameof(IsRedneckTabEnabled));
        }
        else if (parameterName.Equals(nameof(_config.GamePathSlave)))
        {
            OnPropertyChanged(nameof(IsSlaveTabEnabled));
        }
    }
}
