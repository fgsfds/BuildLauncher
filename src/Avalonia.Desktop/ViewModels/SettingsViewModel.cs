using Avalonia.Desktop.Helpers;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Common.Client.Enums;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using Common.Common.Helpers;
using Common.Enums;
using Common.Enums.Versions;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Games.Providers;
using System.Diagnostics;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IConfigProvider _config;
    private readonly GamesPathsProvider _gamesPathsProvider;

    public SettingsViewModel(
        IConfigProvider config,
        GamesPathsProvider gamesPathsProvider
        )
    {
        _config = config;
        _gamesPathsProvider = gamesPathsProvider;
    }


    #region Binding Properties

    public bool IsDefaultTheme => _config.Theme is ThemeEnum.System;

    public bool IsLightTheme => _config.Theme is ThemeEnum.Light;

    public bool IsDarkTheme => _config.Theme is ThemeEnum.Dark;


    public string? PathToBlood
    {
        get => _config.PathBlood;
        set => _config.PathBlood = value;
    }

    public string? PathToDuke3D
    {
        get => _config.PathDuke3D;
        set => _config.PathDuke3D = value;
    }

    public string? PathToDukeWT
    {
        get => _config.PathDukeWT;
        set => _config.PathDukeWT = value;
    }

    public string? PathToDuke64
    {
        get => _config.PathDuke64;
        set => _config.PathDuke64 = value;
    }

    public string? PathToWang
    {
        get => _config.PathWang;
        set => _config.PathWang = value;
    }

    public string? PathToFury
    {
        get => _config.PathFury;
        set => _config.PathFury = value;
    }

    public string? PathToRedneck
    {
        get => _config.PathRedneck;
        set => _config.PathRedneck = value;
    }

    public string? PathToAgain
    {
        get => _config.PathRidesAgain;
        set => _config.PathRidesAgain = value;
    }

    public string? PathToSlave
    {
        get => _config.PathSlave;
        set => _config.PathSlave = value;
    }

    public string? PathToNam
    {
        get => _config.PathNam;
        set => _config.PathNam = value;
    }

    public string? PathToWW2GI
    {
        get => _config.PathWW2GI;
        set => _config.PathWW2GI = value;
    }

    public string? PathToWitchaven
    {
        get => _config.PathWitchaven;
        set => _config.PathWitchaven = value;
    }

    public string? PathToWitchaven2
    {
        get => _config.PathWitchaven2;
        set => _config.PathWitchaven2 = value;
    }

    public string? PathToTekWar
    {
        get => _config.PathTekWar;
        set => _config.PathTekWar = value;
    }

    /// <summary>
    /// Skip intro parameter
    /// </summary>
    public bool SkipIntroCheckbox
    {
        get => _config.SkipIntro;
        set
        {
            _config.SkipIntro = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Skip startup window parameter
    /// </summary>
    public bool SkipStartupCheckbox
    {
        get => _config.SkipStartup;
        set
        {
            _config.SkipStartup = value;
            OnPropertyChanged();
        }
    }

    #endregion


    #region Relay Commands

    [RelayCommand]
    private void SetTheme(object? param)
    {
        Guard.IsNotNull(Application.Current);

        if (!EnumHelper.TryParse<ThemeEnum>(param, out var themeEnum))
        {
            ThrowHelper.ThrowInvalidOperationException();
        }

        Application.Current.RequestedThemeVariant = themeEnum.Value switch
        {
            ThemeEnum.System => ThemeVariant.Default,
            ThemeEnum.Light => ThemeVariant.Light,
            ThemeEnum.Dark => ThemeVariant.Dark,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<ThemeVariant>(),
        };

        _config.Theme = themeEnum.Value;
    }


    /// <summary>
    /// Open folder picker
    /// </summary>
    [RelayCommand]
    private async Task OpenFolderPickerAsync(string param)
    {
        var files = await AvaloniaProperties.TopLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Choose game folder",
                AllowMultiple = false
            }).ConfigureAwait(true);

        if (files.Count == 0)
        {
            return;
        }

        if (param.Equals(nameof(GameEnum.Duke3D)))
        {
            PathToDuke3D = files[0].Path.LocalPath;

            OnPropertyChanged(nameof(PathToDuke3D));
        }
        else if (param.Equals(nameof(DukeVersionEnum.Duke3D_WT)))
        {
            PathToDukeWT = files[0].Path.LocalPath;

            OnPropertyChanged(nameof(PathToDukeWT));
        }
        else if (param.Equals(nameof(GameEnum.Blood)))
        {
            PathToBlood = files[0].Path.LocalPath;

            OnPropertyChanged(nameof(PathToBlood));
        }
        else if (param.Equals(nameof(GameEnum.Wang)))
        {
            PathToWang = files[0].Path.LocalPath;

            OnPropertyChanged(nameof(PathToWang));
        }
        else if (param.Equals(nameof(GameEnum.Redneck)))
        {
            PathToRedneck = files[0].Path.LocalPath;

            OnPropertyChanged(nameof(PathToRedneck));
        }
        else if (param.Equals(nameof(GameEnum.RidesAgain)))
        {
            PathToAgain = files[0].Path.LocalPath;

            OnPropertyChanged(nameof(PathToAgain));
        }
        else if (param.Equals(nameof(GameEnum.Fury)))
        {
            PathToFury = files[0].Path.LocalPath;

            OnPropertyChanged(nameof(PathToFury));
        }
        else if (param.Equals(nameof(GameEnum.Slave)))
        {
            PathToSlave = files[0].Path.LocalPath;

            OnPropertyChanged(nameof(PathToSlave));
        }
        else if (param.Equals(nameof(GameEnum.NAM)))
        {
            PathToNam = files[0].Path.LocalPath;

            OnPropertyChanged(nameof(PathToNam));
        }
        else if (param.Equals(nameof(GameEnum.WW2GI)))
        {
            PathToWW2GI = files[0].Path.LocalPath;

            OnPropertyChanged(nameof(PathToWW2GI));
        }
        else if (param.Equals(nameof(GameEnum.Witchaven)))
        {
            PathToWitchaven = files[0].Path.LocalPath;

            OnPropertyChanged(nameof(PathToWitchaven));
        }
        else if (param.Equals(nameof(GameEnum.Witchaven2)))
        {
            PathToWitchaven2 = files[0].Path.LocalPath;

            OnPropertyChanged(nameof(PathToWitchaven2));
        }
        else if (param.Equals(nameof(GameEnum.TekWar)))
        {
            PathToTekWar = files[0].Path.LocalPath;

            OnPropertyChanged(nameof(PathToTekWar));
        }
    }


    /// <summary>
    /// Replace path to game with auto detected one
    /// </summary>
    [RelayCommand]
    private void Autodetect(string param)
    {
        if (param.Equals(nameof(GameEnum.Duke3D)))
        {
            var path = _gamesPathsProvider.GetPath(GameEnum.Duke3D);

            if (path is not null)
            {
                PathToDuke3D = path;

                OnPropertyChanged(nameof(PathToDuke3D));
            }
        }
        else if (param.Equals(nameof(DukeVersionEnum.Duke3D_WT)))
        {
            var path = _gamesPathsProvider.GetPath(DukeVersionEnum.Duke3D_WT);

            if (path is not null)
            {
                PathToDukeWT = path;

                OnPropertyChanged(nameof(PathToDukeWT));
            }
        }
        else if (param.Equals(nameof(GameEnum.Blood)))
        {
            var path = _gamesPathsProvider.GetPath(GameEnum.Blood);

            if (path is not null)
            {
                PathToBlood = path;

                OnPropertyChanged(nameof(PathToBlood));
            }
        }
        else if (param.Equals(nameof(GameEnum.Wang)))
        {
            var path = _gamesPathsProvider.GetPath(GameEnum.Wang);

            if (path is not null)
            {
                PathToWang = path;

                OnPropertyChanged(nameof(PathToWang));
            }
        }
        else if (param.Equals(nameof(GameEnum.Redneck)))
        {
            var path = _gamesPathsProvider.GetPath(GameEnum.Redneck);

            if (path is not null)
            {
                PathToRedneck = path;

                OnPropertyChanged(nameof(PathToRedneck));
            }
        }
        else if (param.Equals(nameof(GameEnum.RidesAgain)))
        {
            var path = _gamesPathsProvider.GetPath(GameEnum.RidesAgain);

            if (path is not null)
            {
                PathToAgain = path;

                OnPropertyChanged(nameof(PathToAgain));
            }
        }
        else if (param.Equals(nameof(GameEnum.Fury)))
        {
            var path = _gamesPathsProvider.GetPath(GameEnum.Fury);

            if (path is not null)
            {
                PathToFury = path;

                OnPropertyChanged(nameof(PathToFury));
            }
        }
        else if (param.Equals(nameof(GameEnum.Slave)))
        {
            var path = _gamesPathsProvider.GetPath(GameEnum.Slave);

            if (path is not null)
            {
                PathToSlave = path;

                OnPropertyChanged(nameof(PathToSlave));
            }
        }
        else if (param.Equals(nameof(GameEnum.NAM)))
        {
            var path = _gamesPathsProvider.GetPath(GameEnum.NAM);

            if (path is not null)
            {
                PathToNam = path;

                OnPropertyChanged(nameof(PathToNam));
            }
        }
        else if (param.Equals(nameof(GameEnum.WW2GI)))
        {
            var path = _gamesPathsProvider.GetPath(GameEnum.WW2GI);

            if (path is not null)
            {
                PathToWW2GI = path;

                OnPropertyChanged(nameof(PathToWW2GI));
            }
        }
        else if (param.Equals(nameof(GameEnum.Witchaven)))
        {
            var path = _gamesPathsProvider.GetPath(GameEnum.Witchaven);

            if (path is not null)
            {
                PathToWitchaven = path;

                OnPropertyChanged(nameof(PathToWitchaven));
            }
        }
        else if (param.Equals(nameof(GameEnum.Witchaven2)))
        {
            var path = _gamesPathsProvider.GetPath(GameEnum.Witchaven2);

            if (path is not null)
            {
                PathToWitchaven2 = path;

                OnPropertyChanged(nameof(PathToWitchaven2));
            }
        }
        else if (param.Equals(nameof(GameEnum.TekWar)))
        {
            var path = _gamesPathsProvider.GetPath(GameEnum.TekWar);

            if (path is not null)
            {
                PathToTekWar = path;

                OnPropertyChanged(nameof(PathToTekWar));
            }
        }
    }


    /// <summary>
    /// Open file picker
    /// </summary>
    [RelayCommand]
    private async Task OpenFilePickerAsync()
    {
        FilePickerFileType z64 = new("N64 ROM")
        {
            Patterns = ["*.z64", "*.n64"]
        };

        var files = await AvaloniaProperties.TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Choose game folder",
                AllowMultiple = false,
                FileTypeFilter = [z64]
            }).ConfigureAwait(true);

        if (files.Count == 0)
        {
            return;
        }

        PathToDuke64 = files[0].Path.LocalPath;

        OnPropertyChanged(nameof(PathToDuke64));
    }


    /// <summary>
    /// Open log file
    /// </summary>
    [RelayCommand]
    private void OpenLog()
    {
        if (File.Exists(ClientProperties.PathToLogFile))
        {
            using var _ = Process.Start(new ProcessStartInfo
            {
                FileName = ClientProperties.PathToLogFile,
                UseShellExecute = true
            });
        }
    }

    #endregion
}
