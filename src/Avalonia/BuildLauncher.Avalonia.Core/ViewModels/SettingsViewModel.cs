using Avalonia;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using BuildLauncher.Helpers;
using Common.Config;
using Common.Enums;
using Common.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BuildLauncher.ViewModels
{
    internal sealed partial class SettingsViewModel : ObservableObject
    {
        private readonly ConfigEntity _config;

        public SettingsViewModel(ConfigProvider config)
        {
            _config = config.Config;
        }


        #region Binding Properties

        public bool IsDefaultTheme => _config.Theme is ThemeEnum.System;

        public bool IsLightTheme => _config.Theme is ThemeEnum.Light;

        public bool IsDarkTheme => _config.Theme is ThemeEnum.Dark;


        public string PathToBlood
        {
            get => _config.GamePathBlood;
            set => _config.GamePathBlood = value;
        }

        public string PathToDuke3D
        {
            get => _config.GamePathDuke3D;
            set => _config.GamePathDuke3D = value;
        }

        public string PathToDukeWT
        {
            get => _config.GamePathDukeWT;
            set => _config.GamePathDukeWT = value;
        }

        public string PathToDuke64
        {
            get => _config.GamePathDuke64;
            set => _config.GamePathDuke64 = value;
        }

        public string PathToWang
        {
            get => _config.GamePathWang;
            set => _config.GamePathWang = value;
        }

        public string PathToFury
        {
            get => _config.GamePathFury;
            set => _config.GamePathFury = value;
        }

        public string PathToRedneck
        {
            get => _config.GamePathRedneck;
            set => _config.GamePathRedneck = value;
        }

        public string PathToAgain
        {
            get => _config.GamePathAgain;
            set => _config.GamePathAgain = value;
        }

        public string PathToSlave
        {
            get => _config.GamePathSlave;
            set => _config.GamePathSlave = value;
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
                OnPropertyChanged(nameof(SkipIntroCheckbox));
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
                OnPropertyChanged(nameof(SkipStartupCheckbox));
            }
        }

        #endregion


        #region Relay Commands

        [RelayCommand]
        private void SetDefaultTheme()
        {
            Application.Current.ThrowIfNull();

            Application.Current.RequestedThemeVariant = ThemeVariant.Default;
            _config.Theme = ThemeEnum.System;
        }


        [RelayCommand]
        private void SetLightTheme()
        {
            Application.Current.ThrowIfNull();

            Application.Current.RequestedThemeVariant = ThemeVariant.Light;
            _config.Theme = ThemeEnum.Light;
        }


        [RelayCommand]
        private void SetDarkTheme()
        {
            Application.Current.ThrowIfNull();

            Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
            _config.Theme = ThemeEnum.Dark;
        }


        /// <summary>
        /// Open folder picker
        /// </summary>
        [RelayCommand]
        private async Task OpenFolderPickerAsync(string param)
        {
            var files = await Properties.TopLevel.StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions
                {
                    Title = "Choose game folder",
                    AllowMultiple = false
                }).ConfigureAwait(false);

            if (files.Count == 0)
            {
                return;
            }

            if (param.Equals(GameEnum.Blood.ToString()))
            {
                PathToBlood = files[0].Path.LocalPath;

                OnPropertyChanged(nameof(PathToBlood));
            }
            else if (param.Equals(GameEnum.Duke3D.ToString()))
            {
                PathToDuke3D = files[0].Path.LocalPath;

                OnPropertyChanged(nameof(PathToDuke3D));
            }
            else if (param.Equals("DukeWT"))
            {
                PathToDukeWT = files[0].Path.LocalPath;

                OnPropertyChanged(nameof(PathToDuke3D));
            }
            else if (param.Equals(GameEnum.Wang.ToString()))
            {
                PathToWang = files[0].Path.LocalPath;

                OnPropertyChanged(nameof(PathToWang));
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
                Patterns = new[] { "*.z64" }
            };

            var files = await Properties.TopLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Choose game folder",
                    AllowMultiple = false,
                    FileTypeFilter = [z64]
                }).ConfigureAwait(false);

            if (files.Count == 0)
            {
                return;
            }

            PathToDuke64 = files[0].Path.LocalPath;

            OnPropertyChanged(nameof(PathToDuke64));
        }

        #endregion
    }
}
