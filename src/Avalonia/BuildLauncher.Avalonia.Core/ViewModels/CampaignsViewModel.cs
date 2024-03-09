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
    public sealed partial class CampaignsViewModel : ObservableObject, IPortsButtonControl
    {
        public readonly IGame Game;
        private readonly GamesProvider _gamesProvider;
        private readonly DownloadableModsProvider _downloadableModsProvider;
        private readonly ConfigEntity _config;

        public CampaignsViewModel(
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
        public ImmutableList<IMod> CampaignsList => Game.GetCampaigns();

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

        public string SelectedCampaignDescription => SelectedCampaign is null ? string.Empty : SelectedCampaign.ToMarkdownString();

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
        private void StartCampaign(object? command)
        {
            command.ThrowIfNotType<BasePort>(out var port);
            SelectedCampaign.ThrowIfNull();

            var args = port.GetStartGameArgs(Game, SelectedCampaign, SkipIntroCheckbox);

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

            File.Delete(SelectedCampaign.PathToFile);
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
        }

        private void NotifyModDownloaded(ModTypeEnum modType)
        {
            if (modType is ModTypeEnum.Campaign)
            {
                OnPropertyChanged(nameof(CampaignsList));
            }
        }
    }
}
