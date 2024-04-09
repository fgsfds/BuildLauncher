using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mods.Serializable;
using System.Collections.Immutable;

namespace BuildLauncher.ViewModels
{
    public sealed partial class DownloadsViewModel : ObservableObject
    {
        public readonly IGame Game;


        [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
        public DownloadsViewModel(IGame game)
        {
            Game = game;

            Game.DownloadableModsProvider.ModDownloadedEvent += OnModChanged;
            Game.InstalledModsProvider.ModDeletedEvent += OnModChanged;
        }


        #region Binding Properties

        /// <summary>
        /// List of downloadanle campaigns and maps
        /// </summary>
        public ImmutableList<IDownloadableMod> DownloadableCampaignsList => Game.DownloadableModsProvider.GetDownloadableMods(ModTypeEnum.TC);

        /// <summary>
        /// List of downloadanle campaigns and maps
        /// </summary>
        public ImmutableList<IDownloadableMod> DownloadableMapsList => Game.DownloadableModsProvider.GetDownloadableMods(ModTypeEnum.Map);

        /// <summary>
        /// List of downloadanle autoload mods
        /// </summary>
        public ImmutableList<IDownloadableMod> DownloadableModsList => Game.DownloadableModsProvider.GetDownloadableMods(ModTypeEnum.Mod);

        /// <summary>
        /// Download/install progress
        /// </summary>
        public float ProgressBarValue { get; set; }

        /// <summary>
        /// Currently selected downloadable campaign, map or mod
        /// </summary>
        private DownloadableAddonDto? _selectedDownloadableMod;
        public DownloadableAddonDto? SelectedDownloadableMod
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
                OnPropertyChanged(nameof(DownloadButtonText));

                DownloadModCommand.NotifyCanExecuteChanged();
            }
        }

        public string SelectedDownloadableDescription => SelectedDownloadableMod is null ? string.Empty : SelectedDownloadableMod.ToMarkdownString();

        public string DownloadButtonText
        {
            get
            {
                if (SelectedDownloadableMod is null)
                {
                    return "Download";
                }
                else
                {
                    return $"Download ({SelectedDownloadableMod.FileSize.ToSizeString()})";
                }
            }
        }

        #endregion


        #region Relay Commands

        /// <summary>
        /// VM initialization
        /// </summary>
        public Task InitializeAsync() => UpdateAsync();

        /// <summary>
        /// Update downloadable mods list
        /// </summary>
        private async Task UpdateAsync()
        {
            await Game.DownloadableModsProvider.CreateCacheAsync().ConfigureAwait(true);

            OnPropertyChanged(nameof(DownloadableCampaignsList));
            OnPropertyChanged(nameof(DownloadableMapsList));
            OnPropertyChanged(nameof(DownloadableModsList));
        }


        /// <summary>
        /// Download selecred mod
        /// </summary>
        [RelayCommand(CanExecute = (nameof(DownloadSelectedModCanExecute)))]
        private async Task DownloadMod()
        {
            SelectedDownloadableMod.ThrowIfNull();

            Game.DownloadableModsProvider.Progress.ProgressChanged += OnProgressChanged;

            await Game.DownloadableModsProvider.DownloadModAsync(SelectedDownloadableMod).ConfigureAwait(false);

            Game.DownloadableModsProvider.Progress.ProgressChanged -= OnProgressChanged;
            OnProgressChanged(null, 0);
        }
        private bool DownloadSelectedModCanExecute => SelectedDownloadableMod is not null;

        #endregion


        private void OnProgressChanged(object? sender, float e)
        {
            ProgressBarValue = e;
            OnPropertyChanged(nameof(ProgressBarValue));
        }

        private void OnModChanged(IGame game, ModTypeEnum modType)
        {
            if (game.GameEnum != Game.GameEnum)
            {
                return;
            }

            if (modType is ModTypeEnum.TC)
            {
                OnPropertyChanged(nameof(DownloadableCampaignsList));
            }
            else if (modType is ModTypeEnum.Map)
            {
                OnPropertyChanged(nameof(DownloadableMapsList));
            }
            else if (modType is ModTypeEnum.Mod)
            {
                OnPropertyChanged(nameof(DownloadableModsList));
            }
        }
    }
}
