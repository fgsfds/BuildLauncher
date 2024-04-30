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

            Game.DownloadableAddonsProvider.AddonDownloadedEvent += OnAddonChanged;
            Game.InstalledAddonsProvider.AddonDeletedEvent += OnAddonChanged;
        }


        #region Binding Properties

        /// <summary>
        /// List of downloadable campaigns
        /// </summary>
        public ImmutableList<IDownloadableAddon> DownloadableCampaignsList => Game.DownloadableAddonsProvider.GetDownloadableAddons(AddonTypeEnum.TC);

        /// <summary>
        /// List of downloadable maps
        /// </summary>
        public ImmutableList<IDownloadableAddon> DownloadableMapsList => Game.DownloadableAddonsProvider.GetDownloadableAddons(AddonTypeEnum.Map);

        /// <summary>
        /// List of downloadable autoload mods
        /// </summary>
        public ImmutableList<IDownloadableAddon> DownloadableModsList => Game.DownloadableAddonsProvider.GetDownloadableAddons(AddonTypeEnum.Mod);

        /// <summary>
        /// Download/install progress
        /// </summary>
        public float ProgressBarValue { get; set; }

        /// <summary>
        /// Currently selected downloadable campaign, map or mod
        /// </summary>
        private DownloadableAddonDto? _selectedDownloadableAddon;
        public DownloadableAddonDto? SelectedDownloadableAddon
        {
            get => _selectedDownloadableAddon;
            set
            {
                if (value is null)
                {
                    return;
                }

                _selectedDownloadableAddon = value;
                OnPropertyChanged(nameof(SelectedDownloadableAddon));
                OnPropertyChanged(nameof(SelectedDownloadableDescription));
                OnPropertyChanged(nameof(DownloadButtonText));

                DownloadAddonCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Description of the selected addom
        /// </summary>
        public string SelectedDownloadableDescription => SelectedDownloadableAddon is null ? string.Empty : SelectedDownloadableAddon.ToMarkdownString();

        /// <summary>
        /// Text of the download button
        /// </summary>
        public string DownloadButtonText
        {
            get
            {
                if (SelectedDownloadableAddon is null)
                {
                    return "Download";
                }
                else
                {
                    return $"Download ({SelectedDownloadableAddon.FileSize.ToSizeString()})";
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
        /// Update downloadable addons list
        /// </summary>
        private async Task UpdateAsync()
        {
            await Game.DownloadableAddonsProvider.CreateCacheAsync().ConfigureAwait(true);

            OnPropertyChanged(nameof(DownloadableCampaignsList));
            OnPropertyChanged(nameof(DownloadableMapsList));
            OnPropertyChanged(nameof(DownloadableModsList));
        }


        /// <summary>
        /// Download selected addon
        /// </summary>
        [RelayCommand(CanExecute = (nameof(DownloadSelectedAddonCanExecute)))]
        private async Task DownloadAddon()
        {
            SelectedDownloadableAddon.ThrowIfNull();

            Game.DownloadableAddonsProvider.Progress.ProgressChanged += OnProgressChanged;

            await Game.DownloadableAddonsProvider.DownloadAddonAsync(SelectedDownloadableAddon).ConfigureAwait(false);

            Game.DownloadableAddonsProvider.Progress.ProgressChanged -= OnProgressChanged;
            OnProgressChanged(null, 0);
        }
        private bool DownloadSelectedAddonCanExecute => SelectedDownloadableAddon is not null;

        #endregion


        private void OnProgressChanged(object? sender, float e)
        {
            ProgressBarValue = e;
            OnPropertyChanged(nameof(ProgressBarValue));
        }

        private void OnAddonChanged(IGame game, AddonTypeEnum modType)
        {
            if (game.GameEnum != Game.GameEnum)
            {
                return;
            }

            if (modType is AddonTypeEnum.TC)
            {
                OnPropertyChanged(nameof(DownloadableCampaignsList));
            }
            else if (modType is AddonTypeEnum.Map)
            {
                OnPropertyChanged(nameof(DownloadableMapsList));
            }
            else if (modType is AddonTypeEnum.Mod)
            {
                OnPropertyChanged(nameof(DownloadableModsList));
            }
        }
    }
}
