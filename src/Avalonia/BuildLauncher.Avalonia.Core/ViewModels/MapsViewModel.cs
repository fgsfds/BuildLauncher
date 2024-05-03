using Common.Config;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Common.Providers;
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
        private readonly PlaytimeProvider _playtimeProvider;


        [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
        public MapsViewModel(
            IGame game,
            GamesProvider gamesProvider,
            ConfigEntity config,
            PlaytimeProvider playtimeProvider
            )
        {
            Game = game;

            _gamesProvider = gamesProvider;
            _config = config;
            _playtimeProvider = playtimeProvider;

            _gamesProvider.GameChangedEvent += OnGameChanged;
            Game.DownloadableAddonsProvider.AddonDownloadedEvent += OnAddonDownloaded;
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
            await Game.InstalledAddonsProvider.CreateCache(createNew);

            OnPropertyChanged(nameof(MapsList));
        }


        #region Binding Properties

        /// <summary>
        /// List of installed maps
        /// </summary>
        public ImmutableList<IAddon> MapsList
        {
            get
            {
                var result = Game.GetSingleMaps().Select(x => x.Value);

                if (string.IsNullOrWhiteSpace(SearchBoxText))
                {
                    return [.. result];
                }

                return [.. result.Where(x => x.Title.Contains(SearchBoxText, StringComparison.CurrentCultureIgnoreCase))];
            }
        }

        /// <summary>
        /// Description of the selected map
        /// </summary>
        public string SelectedMapDescription => SelectedMap is null ? string.Empty : SelectedMap.ToMarkdownString();

        /// <summary>
        /// Currently selected map
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedMapDescription))]
        [NotifyCanExecuteChangedFor(nameof(StartMapCommand))]
        private IAddon? _selectedMap;

        /// <summary>
        /// Search box text
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MapsList))]
        [NotifyCanExecuteChangedFor(nameof(ClearSearchBoxCommand))]
        private string _searchBoxText;

        #endregion


        #region Relay Commands

        /// <summary>
        /// Start selected map
        /// </summary>
        /// <param name="command">Port to start map with</param>
        [RelayCommand]
        private async Task StartMapAsync(object? command)
        {
            command.ThrowIfNotType<BasePort>(out var port);
            SelectedMap.ThrowIfNull();

            var args = port.GetStartGameArgs(Game, SelectedMap, _config.SkipIntro, _config.SkipStartup);

            await StartPortAsync(SelectedMap.Id, port.FullPathToExe, args);
        }

        private void StartPortAsync(string fullPathToExe, string args)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Open maps folder
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
        /// Refresh maps list
        /// </summary>
        [RelayCommand]
        private async Task RefreshListAsync()
        {
            await UpdateAsync(true);
        }


        /// <summary>
        /// Delete selected map
        /// </summary>
        [RelayCommand]
        private void DeleteMap()
        {
            SelectedMap.ThrowIfNull();

            Game.InstalledAddonsProvider.DeleteAddon(SelectedMap);

            OnPropertyChanged(nameof(MapsList));
        }


        /// <summary>
        /// Clear search bar
        /// </summary>
        [RelayCommand(CanExecute = nameof(ClearSearchBoxCanExecute))]
        private void ClearSearchBox() => SearchBoxText = string.Empty;
        private bool ClearSearchBoxCanExecute() => !string.IsNullOrEmpty(SearchBoxText);

        #endregion


        /// <summary>
        /// Start port with command line args
        /// </summary>
        /// <param name="exe">Path to port exe</param>
        /// <param name="args">Command line arguments</param>
        private async Task StartPortAsync(string id, string exe, string args)
        {
            var sw = Stopwatch.StartNew();

            await Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = true,
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName(exe)
            })!.WaitForExitAsync();

            sw.Stop();
            var time = sw.Elapsed;

            _playtimeProvider.AddTime(id, time);
            SelectedMap!.UpdatePlaytime(time);

            OnPropertyChanged(nameof(SelectedMapDescription));
        }


        private void OnGameChanged(GameEnum parameterName)
        {
            if (parameterName == Game.GameEnum)
            {
                OnPropertyChanged(nameof(MapsList));
            }
        }

        private void OnAddonDownloaded(IGame game, AddonTypeEnum addonType)
        {
            if (game.GameEnum != Game.GameEnum ||
                addonType is not AddonTypeEnum.Map)
            {
                return;
            }

            OnPropertyChanged(nameof(MapsList));
        }
    }
}
