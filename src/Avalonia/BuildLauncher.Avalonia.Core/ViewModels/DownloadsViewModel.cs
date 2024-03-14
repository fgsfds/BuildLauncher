using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mods.Providers;
using Mods.Serializable;
using System.Collections.Immutable;

namespace BuildLauncher.ViewModels
{
    public sealed partial class DownloadsViewModel : ObservableObject
    {
        public readonly IGame Game;
        private readonly DownloadableModsProvider _downloadableModsProvider;
        private readonly InstalledModsProvider _installedModsProvider;

        public DownloadsViewModel(
            IGame game,
            DownloadableModsProvider downloadableModsProvider,
            InstalledModsProvider installedModsProvider
            )
        {
            Game = game;

            _downloadableModsProvider = downloadableModsProvider;
            _installedModsProvider = installedModsProvider;

            _downloadableModsProvider.NotifyModDownloaded += ModChanged;
            _installedModsProvider.NotifyModDeleted += ModChanged;
        }


        #region Binding Properties

        /// <summary>
        /// List of downloadanle campaigns and maps
        /// </summary>
        public ImmutableList<DownloadableMod> DownloadableCampaignsList => _downloadableModsProvider.GetDownloadableMods(Game, ModTypeEnum.Campaign);

        /// <summary>
        /// List of downloadanle campaigns and maps
        /// </summary>
        public ImmutableList<DownloadableMod> DownloadableMapsList => _downloadableModsProvider.GetDownloadableMods(Game, ModTypeEnum.Map);

        /// <summary>
        /// List of downloadanle autoload mods
        /// </summary>
        public ImmutableList<DownloadableMod> DownloadableModsList => _downloadableModsProvider.GetDownloadableMods(Game, ModTypeEnum.Autoload);

        /// <summary>
        /// Download/install progress
        /// </summary>
        public float ProgressBarValue { get; set; }

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
            await _downloadableModsProvider.UpdateCachedListAsync().ConfigureAwait(false);

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

            _downloadableModsProvider.Progress.ProgressChanged += ProgressChanged;

            await _downloadableModsProvider.DownloadModAsync(SelectedDownloadableMod, Game).ConfigureAwait(false); ;

            _downloadableModsProvider.Progress.ProgressChanged -= ProgressChanged;
            ProgressChanged(null, 0);
        }
        private bool DownloadSelectedModCanExecute => SelectedDownloadableMod is not null;

        #endregion


        private void ProgressChanged(object? sender, float e)
        {
            ProgressBarValue = e;
            OnPropertyChanged(nameof(ProgressBarValue));
        }

        private void ModChanged(IGame game, ModTypeEnum modType)
        {
            if (game.GameEnum != Game.GameEnum)
            {
                return;
            }

            if (modType is ModTypeEnum.Campaign)
            {
                OnPropertyChanged(nameof(DownloadableCampaignsList));
            }
            else if (modType is ModTypeEnum.Map)
            {
                OnPropertyChanged(nameof(DownloadableMapsList));
            }
            else if (modType is ModTypeEnum.Autoload)
            {
                OnPropertyChanged(nameof(DownloadableModsList));
            }
        }
    }
}
