using ClientCommon.Helpers;
using Common.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using Games.Providers;

namespace BuildLauncher.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly GamesProvider _gamesProvider;

    public MainViewModel(
        GamesProvider gamesProvider)
    {
        _gamesProvider = gamesProvider;

        _gamesProvider.GameChangedEvent += OnGameChanged;
    }


    #region Binding Properties

    /// <summary>
    /// Is Blood tab enabled
    /// </summary>
    public bool IsBloodTabEnabled => _gamesProvider.IsBloodInstalled;

    /// <summary>
    /// Is Duke Nukem 3D tab enabled
    /// </summary>
    public bool IsDukeTabEnabled => _gamesProvider.IsDukeInstalled;

    /// <summary>
    /// Is Shadow Warrior tab enabled
    /// </summary>
    public bool IsWangTabEnabled => _gamesProvider.IsWangInstalled;

    /// <summary>
    /// Is Ion Fury tab enabled
    /// </summary>
    public bool IsFuryTabEnabled => _gamesProvider.IsFuryInstalled;

    /// <summary>
    /// Is Redneck Rampage tab enabled
    /// </summary>
    public bool IsRedneckTabEnabled => _gamesProvider.IsRedneckInstalled;

    /// <summary>
    /// Is Powerslave tab enabled
    /// </summary>
    public bool IsSlaveTabEnabled => _gamesProvider.IsSlaveInstalled;

    public bool IsDevMode => ClientProperties.IsDevMode;

    #endregion


    /// <summary>
    /// Update VM with path to the game changes in the config
    /// </summary>
    private void OnGameChanged(GameEnum _)
    {
        OnPropertyChanged(nameof(IsBloodTabEnabled));
        OnPropertyChanged(nameof(IsDukeTabEnabled));
        OnPropertyChanged(nameof(IsWangTabEnabled));
        OnPropertyChanged(nameof(IsFuryTabEnabled));
        OnPropertyChanged(nameof(IsRedneckTabEnabled));
        OnPropertyChanged(nameof(IsSlaveTabEnabled));
    }
}
