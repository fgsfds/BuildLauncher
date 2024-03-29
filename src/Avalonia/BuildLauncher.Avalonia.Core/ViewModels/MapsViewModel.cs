using Common.Config;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Games.Providers;
using Ports.Ports;
using System.Collections.Immutable;
using System.Diagnostics;

namespace BuildLauncher.ViewModels
{
    public sealed partial class MapsViewModel : ObservableObject, IPortsButtonControl
    {
        public readonly IGame Game;
        private readonly GamesProvider _gamesProvider;
        private readonly ConfigEntity _config;


        [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
        public MapsViewModel(
            IGame game,
            GamesProvider gamesProvider,
            ConfigEntity config
            )
        {
            Game = game;

            _gamesProvider = gamesProvider;
            _config = config;

            _gamesProvider.GameChangedEvent += OnGameChanged;
            Game.DownloadableModsProvider.ModDownloadedEvent += OnModDownloaded;
        }


        /// <summary>
        /// VM initialization
        /// </summary>
        public Task InitializeAsync() => UpdateAsync(false);

        /// <summary>
        /// Update maps list
        /// </summary>
        private async Task UpdateAsync(bool createNew)
        {
            await Game.InstalledModsProvider.CreateCache(createNew);

            OnPropertyChanged(nameof(MapsList));
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

        public string SelectedMapDescription => SelectedMap is null ? string.Empty : SelectedMap.ToMarkdownString();

        #endregion


        #region Relay Commands

        /// <summary>
        /// Start selected map/campaign
        /// </summary>
        /// <param name="command">Port to start map/campaign with</param>
        [RelayCommand]
        private void StartMap(object? command)
        {
            command.ThrowIfNotType<BasePort>(out var port);
            SelectedMap.ThrowIfNull();

            var args = port.GetStartGameArgs(Game, SelectedMap, _config.SkipIntro, _config.SkipStartup);

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
        /// Refresh campaigns list
        /// </summary>
        [RelayCommand]
        private async Task RefreshListAsync()
        {
            await UpdateAsync(true);
        }


        /// <summary>
        /// Delete selected map/campaign
        /// </summary>
        [RelayCommand]
        private void DeleteMap()
        {
            SelectedMap.ThrowIfNull();

            Game.InstalledModsProvider.DeleteMod(SelectedMap);

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


        private void OnGameChanged(GameEnum parameterName)
        {
            if (parameterName == Game.GameEnum)
            {
                OnPropertyChanged(nameof(MapsList));
            }
        }

        private void OnModDownloaded(IGame game, ModTypeEnum modType)
        {
            if (game.GameEnum != Game.GameEnum ||
                modType is not ModTypeEnum.Map)
            {
                return;
            }

            OnPropertyChanged(nameof(MapsList));
        }
    }
}
