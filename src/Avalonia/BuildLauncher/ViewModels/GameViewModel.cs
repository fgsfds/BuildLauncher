using Common.Enums;
using Common.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mods.Mods;
using Games.Providers;
using Ports.Ports;
using System.Collections.Immutable;
using System.Diagnostics;
using Mods;
using Common.Tools;
using Common.Interfaces;
using Mods.Providers;
using System.Text;

namespace BuildLauncher.ViewModels
{
    public sealed partial class GameViewModel : ObservableObject
    {
        public readonly IGame Game;
        private readonly GamesProvider _gamesProvider;
        private readonly ArchiveTools _archiveTools;
        private readonly DownloadableModsProvider _downloadableModsProvider;

        public GameViewModel(
        IGame game,
        GamesProvider gamesProvider,
        ArchiveTools archiveTools,
        DownloadableModsProvider modsProvider)
        {
            Game = game;

            _gamesProvider = gamesProvider;
            _archiveTools = archiveTools;
            _downloadableModsProvider = modsProvider;

            _gamesProvider.NotifyGameChanged += NotifyGameChanged;
        }


        #region Binding Properties

        /// <summary>
        /// List of installed campaigns and maps
        /// </summary>
        public ImmutableList<IMod> CampaignsList => [.. Game.GetCampaigns(), .. Game.GetSingleMaps()];

        /// <summary>
        /// List of installed autoload mods
        /// </summary>
        public ImmutableList<AutoloadMod> ModsList => Game.GetAutoloadMods().ConvertAll(static x => (AutoloadMod)x);

        /// <summary>
        /// List of downloadanle campaigns and maps
        /// </summary>
        public ImmutableList<DownloadableMod> DownloadableCampaignsList => _downloadableModsProvider.GetDownloadableMods(Game.GameEnum, [ModTypeEnum.Campaign, ModTypeEnum.Map]);

        /// <summary>
        /// List of downloadanle autoload mods
        /// </summary>
        public ImmutableList<DownloadableMod> DownloadableModsList => _downloadableModsProvider.GetDownloadableMods(Game.GameEnum, ModTypeEnum.Autoload);

        /// <summary>
        /// Download/install progress
        /// </summary>
        public float ProgressBarValue { get; set; }

        /// <summary>
        /// Skip intro parameter
        /// </summary>
        public bool SkipIntroCheckbox { get; set; }

        /// <summary>
        /// Currently selected campaign/map
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedCampaignDescription))]
        [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
        private IMod? _selectedCampaign;

        /// <summary>
        /// Currently selected autoload mod
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedModDescription))]
        private IMod? _selectedMod;

        /// <summary>
        /// Currently selected downloadable campaign, map or mod
        /// </summary>
        private DownloadableMod? _selectedDownloadableMod;
        public DownloadableMod? SelectedDownloadableMod
        {
            get => _selectedDownloadableMod;
            set
            {
                if (value is null)
                {
                    return;
                }

                _selectedDownloadableMod = value;
                OnPropertyChanged(nameof(SelectedDownloadableMod));
                OnPropertyChanged(nameof(SelectedDownloadableDescription));

                DownloadModCommand.NotifyCanExecuteChanged();
            }
        }

        public string SelectedCampaignDescription => SelectedCampaign is null ? string.Empty : SelectedCampaign.ToMarkdownString();

        public string SelectedModDescription => SelectedMod is null ? string.Empty : SelectedMod.ToMarkdownString();

        public string SelectedDownloadableDescription => SelectedDownloadableMod is null ? string.Empty : SelectedDownloadableMod.ToMarkdownString();

        #endregion


        #region Relay Commands

        /// <summary>
        /// VM initialization
        /// </summary>
        [RelayCommand]
        private Task InitializeAsync() => UpdateAsync(true);

        /// <summary>
        /// Update downloadable mods list
        /// </summary>
        /// <param name="useCache">Update from cache</param>
        private async Task UpdateAsync(bool useCache)
        {
            await _downloadableModsProvider.UpdateCachedListAsync().ConfigureAwait(false);

            OnPropertyChanged(nameof(DownloadableCampaignsList));
            OnPropertyChanged(nameof(DownloadableModsList));
        }


        /// <summary>
        /// Start selected map/campaign
        /// </summary>
        /// <param name="command">Port to start map/campaign with</param>
        [RelayCommand]
        private void StartGame(object? command)
        {
            command.ThrowIfNotType<BasePort>(out var port);
            SelectedCampaign.ThrowIfNull();

            port.BeforeStart(Game);

            StringBuilder sb = new();

            port.GetStartCampaignArgs(sb, Game, SelectedCampaign);
            port.GetAutoloadModsArgs(sb, Game, [.. ModsList]);

            if (SkipIntroCheckbox)
            {
                port.GetSkipIntroParameter(sb);
            }

            var args = sb.ToString();

            StartPort(port.FullPathToExe, args);
        }


        /// <summary>
        /// Open mods folder
        /// </summary>
        [RelayCommand]
        private void OpenModsFolder()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Game.ModsFolderPath,
                UseShellExecute = true,
            });
        }


        /// <summary>
        /// Delete selected mod
        /// </summary>
        [RelayCommand]
        private void DeleteMod()
        {
            SelectedMod.ThrowIfNull();
            SelectedMod.PathToFile.ThrowIfNull();

            File.Delete(SelectedMod.PathToFile);
            OnPropertyChanged(nameof(ModsList));
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


        /// <summary>
        /// Download selecred mod
        /// </summary>
        [RelayCommand(CanExecute = (nameof(DownloadSelectedModCanExecute)))]
        private async Task DownloadMod()
        {
            SelectedDownloadableMod.ThrowIfNull();

            var url = SelectedDownloadableMod.Url;
            var file = Path.GetFileName(url.ToString());
            string path;

            if (SelectedDownloadableMod.ModType is ModTypeEnum.Campaign)
            {
                path = Game.CampaignsFolderPath;
            }
            else if (SelectedDownloadableMod.ModType is ModTypeEnum.Map)
            {
                path = Game.MapsFolderPath;
            }
            else if (SelectedDownloadableMod.ModType is ModTypeEnum.Autoload)
            {
                path = Game.ModsFolderPath;
            }
            else
            {
                ThrowHelper.NotImplementedException(SelectedDownloadableMod.ModType.ToString());
                return;
            }

            _archiveTools.Progress.ProgressChanged += ProgressChanged;

            await _archiveTools.DownloadFileAsync(new(url), Path.Combine(path, file));

            _archiveTools.Progress.ProgressChanged -= ProgressChanged;
            ProgressChanged(null, 0);

            Game.CreateCombinedMod();

            OnPropertyChanged(nameof(CampaignsList));
            OnPropertyChanged(nameof(ModsList));
        }
        private bool DownloadSelectedModCanExecute => SelectedDownloadableMod is not null;

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
            if (parameterName == Game.GameEnum ||
               (parameterName.Equals("DukeWT") && parameterName is GameEnum.Duke3D))
            {
                OnPropertyChanged(nameof(CampaignsList));
                OnPropertyChanged(nameof(ModsList));
            }
        }

        private void ProgressChanged(object? sender, float e)
        {
            ProgressBarValue = e;
            OnPropertyChanged(nameof(ProgressBarValue));
        }
    }
}
