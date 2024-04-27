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
    public sealed partial class CampaignsViewModel : ObservableObject, IPortsButtonControl
    {
        public readonly IGame Game;

        private readonly GamesProvider _gamesProvider;
        private readonly ConfigEntity _config;


        [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
        public CampaignsViewModel(
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


        #region Binding Properties

        /// <summary>
        /// List of installed campaigns and maps
        /// </summary>
        public ImmutableList<IAddon> CampaignsList => Game.GetCampaigns().Select(x => x.Value).ToImmutableList();

        /// <summary>
        /// Currently selected campaign/map
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedCampaignDescription))]
        [NotifyPropertyChangedFor(nameof(SelectedCampaignPreview))]
        [NotifyPropertyChangedFor(nameof(IsPreviewVisible))]
        [NotifyCanExecuteChangedFor(nameof(StartCampaignCommand))]
        private IAddon? _selectedCampaign;

        public string SelectedCampaignDescription => SelectedCampaign is null ? string.Empty : SelectedCampaign.ToMarkdownString();

        public Stream? SelectedCampaignPreview => SelectedCampaign?.Preview;

        public bool IsPreviewVisible => SelectedCampaign?.Preview is not null;

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
            await Game.InstalledModsProvider.CreateCache(createNew);

            OnPropertyChanged(nameof(CampaignsList));
        }


        #region Relay Commands

        /// <summary>
        /// Start selected map/campaign
        /// </summary>
        /// <param name="command">Port to start map/campaign with</param>
        [RelayCommand]
        private void StartCampaign(object? command)
        {
            command.ThrowIfNotType<BasePort>(out var port);
            SelectedCampaign.ThrowIfNull();

            var args = port.GetStartGameArgs(Game, SelectedCampaign, _config.SkipIntro, _config.SkipStartup);

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
        /// Delete selected map/campaign
        /// </summary>
        [RelayCommand]
        private void DeleteCampaign()
        {
            SelectedCampaign.ThrowIfNull();

            Game.InstalledModsProvider.DeleteMod(SelectedCampaign);

            OnPropertyChanged(nameof(CampaignsList));
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
                OnPropertyChanged(nameof(CampaignsList));
            }
        }

        private void OnModDownloaded(IGame game, AddonTypeEnum modType)
        {
            if (game.GameEnum != Game.GameEnum ||
                modType is not AddonTypeEnum.TC)
            {
                return;
            }

            OnPropertyChanged(nameof(CampaignsList));
        }
    }
}
