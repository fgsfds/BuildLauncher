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

        public CampaignsViewModel(
            IGame game,
            GamesProvider gamesProvider,
            ConfigEntity config
            )
        {
            Game = game;

            _gamesProvider = gamesProvider;
            _config = config;

            _gamesProvider.NotifyGameChanged += NotifyGameChanged;
            _config.NotifyParameterChanged += NotifyConfigChanged;
            Game.DownloadableModsProvider.NotifyModDownloaded += NotifyModDownloaded;
        }


        #region Binding Properties

        /// <summary>
        /// List of installed campaigns and maps
        /// </summary>
        public ImmutableList<IMod> CampaignsList => Game.GetCampaigns().Select(x => x.Value).ToImmutableList();

        /// <summary>
        /// Currently selected campaign/map
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedCampaignDescription))]
        [NotifyCanExecuteChangedFor(nameof(StartCampaignCommand))]
        private IMod? _selectedCampaign;

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

        public string SelectedCampaignDescription => SelectedCampaign is null ? string.Empty : SelectedCampaign.ToMarkdownString();

        #endregion


        /// <summary>
        /// VM initialization
        /// </summary>
        public Task InitializeAsync() => UpdateAsync();

        /// <summary>
        /// Update campaigns list
        /// </summary>
        private async Task UpdateAsync()
        {
            await Game.InstalledModsProvider.CreateCache();

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

            var args = port.GetStartGameArgs(Game, SelectedCampaign, SkipIntroCheckbox, SkipStartupCheckbox);

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
        /// Delete selected map/campaign
        /// </summary>
        [RelayCommand]
        private void DeleteCampaign()
        {
            SelectedCampaign.ThrowIfNull();
            SelectedCampaign.PathToFile.ThrowIfNull();

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


        private void NotifyGameChanged(GameEnum parameterName)
        {
            if (parameterName == Game.GameEnum)
            {
                OnPropertyChanged(nameof(CampaignsList));
            }
        }

        private void NotifyConfigChanged(string parameterName)
        {
            if (parameterName.Equals(nameof(_config.SkipIntro)))
            {
                OnPropertyChanged(nameof(SkipIntroCheckbox));
            }
            else if (parameterName.Equals(nameof(_config.SkipStartup)))
            {
                OnPropertyChanged(nameof(SkipStartupCheckbox));
            }
        }

        private void NotifyModDownloaded(IGame game, ModTypeEnum modType)
        {
            if (game.GameEnum != Game.GameEnum ||
                modType is not ModTypeEnum.Campaign)
            {
                return;
            }

            OnPropertyChanged(nameof(CampaignsList));
        }
    }
}
