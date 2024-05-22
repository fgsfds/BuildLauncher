using ClientCommon.API;
using ClientCommon.Config;
using ClientCommon.Providers;
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
    public sealed partial class MapsViewModel : ObservableObject, IPortsButtonControl, IRightPanelControl
    {
        public readonly IGame Game;

        private readonly GamesProvider _gamesProvider;
        private readonly ConfigEntity _config;
        private readonly PlaytimeProvider _playtimeProvider;
        private readonly ApiInterface _apiInterface;
        private readonly ScoresProvider _scoresProvider;


        [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
        public MapsViewModel(
            IGame game,
            GamesProvider gamesProvider,
            ConfigEntity config,
            PlaytimeProvider playtimeProvider,
            ApiInterface apiInterface,
            ScoresProvider scoresProvider
            )
        {
            Game = game;

            _gamesProvider = gamesProvider;
            _config = config;
            _playtimeProvider = playtimeProvider;
            _apiInterface = apiInterface;
            _scoresProvider = scoresProvider;

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
        public string SelectedAddonDescription => SelectedMap is null ? string.Empty : SelectedMap.ToMarkdownString();

        public string? SelectedAddonPlaytime
        {
            get
            {
                if (SelectedMap is null)
                {
                    return null;
                }

                var time = _playtimeProvider.GetTime(SelectedMap.Id);

                if (time is not null)
                {
                    return $"Play time: {time.Value.ToTimeString()}";
                }

                return "Never played";
            }
        }

        public int? SelectedAddonScore
        {
            get
            {
                if (SelectedMap is null)
                {
                    return null;
                }

                var hasUpvote = _scoresProvider.GetScore(SelectedMap.Id);

                if (hasUpvote is not null)
                {
                    return hasUpvote;
                }

                return null;
            }
        }

        public bool IsSelectedAddonUpvoted
        {
            get
            {
                if (SelectedMap is null)
                {
                    return false;
                }

                var hasUpvote = _config.Upvotes.TryGetValue(SelectedMap.Id, out var isUpvote);

                if (hasUpvote && isUpvote)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsSelectedAddonDownvoted
        {
            get
            {
                if (SelectedMap is null)
                {
                    return false;
                }

                var hasUpvote = _config.Upvotes.TryGetValue(SelectedMap.Id, out var isUpvote);

                if (hasUpvote && !isUpvote)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsPreviewVisible => false;

        public Stream? SelectedAddonPreview => null;

        /// <summary>
        /// Currently selected map
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedAddonDescription))]
        [NotifyPropertyChangedFor(nameof(SelectedAddonScore))]
        [NotifyPropertyChangedFor(nameof(IsSelectedAddonUpvoted))]
        [NotifyPropertyChangedFor(nameof(IsSelectedAddonDownvoted))]
        [NotifyPropertyChangedFor(nameof(SelectedAddonPlaytime))]
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


        /// <summary>
        /// Upvote fix
        /// </summary>
        [RelayCommand]
        private async Task Upvote()
        {
            SelectedMap.ThrowIfNull();

            var newScore = await _apiInterface.ChangeVoteAsync(SelectedMap, true).ConfigureAwait(true);
            _scoresProvider.ChangeScore(SelectedMap.Id, newScore);

            OnPropertyChanged(nameof(SelectedAddonScore));
            OnPropertyChanged(nameof(IsSelectedAddonUpvoted));
            OnPropertyChanged(nameof(IsSelectedAddonDownvoted));
        }


        /// <summary>
        /// Downvote fix
        /// </summary>
        [RelayCommand]
        private async Task Downvote()
        {
            SelectedMap.ThrowIfNull();

            var newScore = await _apiInterface.ChangeVoteAsync(SelectedMap, false).ConfigureAwait(true);
            _scoresProvider.ChangeScore(SelectedMap.Id, newScore);

            OnPropertyChanged(nameof(SelectedAddonScore));
            OnPropertyChanged(nameof(IsSelectedAddonUpvoted));
            OnPropertyChanged(nameof(IsSelectedAddonDownvoted));
        }

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

            OnPropertyChanged(nameof(SelectedAddonDescription));
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
