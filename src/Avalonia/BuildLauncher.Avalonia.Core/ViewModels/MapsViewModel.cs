using Common.Config;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Games.Providers;
using Mods.Providers;
using Ports.Ports;
using System.Collections.Immutable;
using System.Diagnostics;

namespace BuildLauncher.ViewModels
{
    public sealed partial class MapsViewModel : ObservableObject, IPortsButtonControl
    {
        public readonly IGame Game;
        private readonly GamesProvider _gamesProvider;
        private readonly DownloadableModsProvider _downloadableModsProvider;
        private readonly ConfigEntity _config;

        public MapsViewModel(
            IGame game,
            GamesProvider gamesProvider,
            DownloadableModsProvider modsProvider,
            ConfigEntity config)
        {
            Game = game;

            _gamesProvider = gamesProvider;
            _downloadableModsProvider = modsProvider;
            _config = config;

            _gamesProvider.NotifyGameChanged += NotifyGameChanged;
            _config.NotifyParameterChanged += NotifyConfigChanged;
            _downloadableModsProvider.NotifyModDownloaded += NotifyModDownloaded;
        }


        #region Binding Properties

        /// <summary>
        /// List of installed campaigns and maps
        /// </summary>
        public ImmutableList<IMod> MapsList => Game.GetSingleMaps().Select(x => x.Value).ToImmutableList();

        /// <summary>
        /// Currently selected campaign/map
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedMapDescription))]
        [NotifyCanExecuteChangedFor(nameof(StartMapCommand))]
        private IMod? _selectedMap;

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

        public string SelectedMapDescription => SelectedMap is null ? string.Empty : SelectedMap.ToMarkdownString();

        #endregion


        #region Relay Commands

        /// <summary>
        /// VM initialization
        /// </summary>
        [RelayCommand]
        private Task InitializeAsync() => Task.CompletedTask;


        /// <summary>
        /// Start selected map/campaign
        /// </summary>
        /// <param name="command">Port to start map/campaign with</param>
        [RelayCommand]
        private void StartMap(object? command)
        {
            command.ThrowIfNotType<BasePort>(out var port);
            SelectedMap.ThrowIfNull();

            var args = port.GetStartGameArgs(Game, SelectedMap, SkipIntroCheckbox);

            StartPort(port.FullPathToExe, args);
        }


        /// <summary>
        /// Open mods folder
        /// </summary>
        [RelayCommand]
        private void OpenFolder()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Game.MapsFolderPath,
                UseShellExecute = true,
            });
        }


        /// <summary>
        /// Delete selected map/campaign
        /// </summary>
        [RelayCommand]
        private void DeleteMap()
        {
            SelectedMap.ThrowIfNull();
            SelectedMap.PathToFile.ThrowIfNull();

            File.Delete(SelectedMap.PathToFile);
            OnPropertyChanged(nameof(MapsList));
        }

        #endregion


        /// <summary>
        /// Start port with command line args
        /// </summary>
        /// <param name="exe">Path to port exe</param>
        /// <param name="args">Command line arguments</param>
        private static void StartPort(string exe, string args)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = true,
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName(exe)
            });
        }


        private void NotifyGameChanged(GameEnum parameterName)
        {
            if (parameterName == Game.GameEnum)
            {
                OnPropertyChanged(nameof(MapsList));
            }
        }

        private void NotifyConfigChanged(string parameterName)
        {
            if (parameterName.Equals(nameof(_config.SkipIntro)))
            {
                OnPropertyChanged(nameof(SkipIntroCheckbox));
            }
        }

        private void NotifyModDownloaded(ModTypeEnum modType)
        {
            if (modType is ModTypeEnum.Map)
            {
                OnPropertyChanged(nameof(MapsList));
            }
        }
    }
}
