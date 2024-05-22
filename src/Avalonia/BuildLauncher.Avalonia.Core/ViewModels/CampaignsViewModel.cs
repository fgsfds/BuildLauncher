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
    public sealed partial class CampaignsViewModel : ObservableObject, IPortsButtonControl, IRightPanelControl
    {
        public readonly IGame Game;

        private readonly GamesProvider _gamesProvider;
        private readonly ConfigEntity _config;
        private readonly PlaytimeProvider _playtimeProvider;
        private readonly ApiInterface _apiInterface;
        private readonly ScoresProvider _scoresProvider;


        [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
        public CampaignsViewModel(
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


        #region Binding Properties

        /// <summary>
        /// List of installed campaigns and maps
        /// </summary>
        public ImmutableList<IAddon> CampaignsList
        {
            get
            {
                var result = Game.GetCampaigns().Select(x => x.Value);

                if (string.IsNullOrWhiteSpace(SearchBoxText))
                {
                    return [.. result];
                }

                return [.. result.Where(x => x.Title.Contains(SearchBoxText, StringComparison.CurrentCultureIgnoreCase))];
            }
        }

        /// <summary>
        /// Description of the selected campaign
        /// </summary>
        public string SelectedAddonDescription => SelectedCampaign is null ? string.Empty : SelectedCampaign.ToMarkdownString();

        /// <summary>
        /// Preview image of the selected campaign
        /// </summary>
        public Stream? SelectedAddonPreview => SelectedCampaign?.PreviewImage;

        /// <summary>
        /// Is preview image in the description visible
        /// </summary>
        public bool IsPreviewVisible => SelectedCampaign?.PreviewImage is not null;

        public int? SelectedAddonScore
        {
            get
            {
                if (SelectedCampaign is null)
                {
                    return null;
                }

                var hasUpvote = _scoresProvider.GetScore(SelectedCampaign.Id);

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
                if (SelectedCampaign is null)
                {
                    return false;
                }

                var hasUpvote = _config.Upvotes.TryGetValue(SelectedCampaign.Id, out var isUpvote);

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
                if (SelectedCampaign is null)
                {
                    return false;
                }

                var hasUpvote = _config.Upvotes.TryGetValue(SelectedCampaign.Id, out var isUpvote);

                if (hasUpvote && !isUpvote)
                {
                    return true;
                }

                return false;
            }
        }


        public string? SelectedAddonPlaytime
        {
            get
            {
                if (SelectedCampaign is null)
                {
                    return null;
                }

                var time = _playtimeProvider.GetTime(SelectedCampaign.Id);

                if (time is not null)
                {
                    return $"Play time: {time.Value.ToTimeString()}";
                }

                return "Never played";
            }
        }

        /// <summary>
        /// Currently selected campaign/map
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedAddonDescription))]
        [NotifyPropertyChangedFor(nameof(SelectedAddonPreview))]
        [NotifyPropertyChangedFor(nameof(IsPreviewVisible))]
        [NotifyPropertyChangedFor(nameof(SelectedAddonScore))]
        [NotifyPropertyChangedFor(nameof(IsSelectedAddonUpvoted))]
        [NotifyPropertyChangedFor(nameof(IsSelectedAddonDownvoted))]
        [NotifyPropertyChangedFor(nameof(SelectedAddonPlaytime))]
        [NotifyCanExecuteChangedFor(nameof(StartCampaignCommand))]
        private IAddon? _selectedCampaign;

        /// <summary>
        /// Search box text
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CampaignsList))]
        [NotifyCanExecuteChangedFor(nameof(ClearSearchBoxCommand))]
        private string _searchBoxText;

        #endregion


        /// <summary>
        /// VM initialization
        /// </summary>
        public Task InitializeAsync() => UpdateAsync(false);

        /// <summary>
        /// Update campaigns list
        /// </summary>
        private async Task UpdateAsync(bool createNew)
        {
            await Game.InstalledAddonsProvider.CreateCache(createNew);

            OnPropertyChanged(nameof(CampaignsList));
        }


        #region Relay Commands

        /// <summary>
        /// Start selected campaign
        /// </summary>
        /// <param name="command">Port to start campaign with</param>
        [RelayCommand]
        private async Task StartCampaignAsync(object? command)
        {
            command.ThrowIfNotType<BasePort>(out var port);
            SelectedCampaign.ThrowIfNull();

            var args = port.GetStartGameArgs(Game, SelectedCampaign, _config.SkipIntro, _config.SkipStartup);

            await StartPortAsync(SelectedCampaign.Id, port.FullPathToExe, args);
        }


        /// <summary>
        /// Open campaigns folder
        /// </summary>
        [RelayCommand]
        private void OpenFolder()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Game.CampaignsFolderPath,
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
        /// Delete selected campaign
        /// </summary>
        [RelayCommand]
        private void DeleteCampaign()
        {
            SelectedCampaign.ThrowIfNull();

            Game.InstalledAddonsProvider.DeleteAddon(SelectedCampaign);

            OnPropertyChanged(nameof(CampaignsList));
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
            SelectedCampaign.ThrowIfNull();
            
            var newScore = await _apiInterface.ChangeVoteAsync(SelectedCampaign, true).ConfigureAwait(true);
            _scoresProvider.ChangeScore(SelectedCampaign.Id, newScore);

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
            SelectedCampaign.ThrowIfNull();

            var newScore = await _apiInterface.ChangeVoteAsync(SelectedCampaign, false).ConfigureAwait(true);
            _scoresProvider.ChangeScore(SelectedCampaign.Id, newScore);

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

            OnPropertyChanged(nameof(SelectedAddonPlaytime));
        }


        private void OnGameChanged(GameEnum parameterName)
        {
            if (parameterName == Game.GameEnum)
            {
                OnPropertyChanged(nameof(CampaignsList));
            }
        }

        private void OnAddonDownloaded(IGame game, AddonTypeEnum addonType)
        {
            if (game.GameEnum != Game.GameEnum ||
                addonType is not AddonTypeEnum.TC)
            {
                return;
            }

            OnPropertyChanged(nameof(CampaignsList));
        }
    }
}
