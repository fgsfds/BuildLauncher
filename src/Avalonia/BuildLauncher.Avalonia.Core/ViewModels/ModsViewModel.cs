using ClientCommon.API;
using ClientCommon.Config;
using ClientCommon.Providers;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Games.Providers;
using Mods.Addons;
using System.Collections.Immutable;
using System.Diagnostics;

namespace BuildLauncher.ViewModels
{
    public sealed partial class ModsViewModel : ObservableObject, IRightPanelControl
    {
        public readonly IGame Game;

        private readonly GamesProvider _gamesProvider;
        private readonly ConfigEntity _config;
        private readonly ApiInterface _apiInterface;
        private readonly ScoresProvider _scoresProvider;


        [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
        public ModsViewModel(
            IGame game,
            GamesProvider gamesProvider,
            ConfigEntity config,
            ApiInterface apiInterface,
            ScoresProvider scoresProvider
            )
        {
            Game = game;

            _gamesProvider = gamesProvider;
            _config = config;
            _apiInterface = apiInterface;
            _scoresProvider = scoresProvider;

            _gamesProvider.GameChangedEvent += OnGameChanged;
            Game.DownloadableAddonsProvider.AddonDownloadedEvent += OnModDownloaded;
        }


        /// <summary>
        /// VM initialization
        /// </summary>
        public Task InitializeAsync() => UpdateAsync(false);

        /// <summary>
        /// Update mods list
        /// </summary>
        private async Task UpdateAsync(bool createNew)
        {
            await Game.InstalledAddonsProvider.CreateCache(createNew);

            OnPropertyChanged(nameof(ModsList));
        }


        #region Binding Properties

        /// <summary>
        /// List of installed autoload mods
        /// </summary>
        public ImmutableList<AutoloadMod> ModsList => [.. Game.GetAutoloadMods(false).Select(x => (AutoloadMod)x.Value)];

        /// <summary>
        /// Description of the selected mod
        /// </summary>
        public string SelectedAddonDescription => SelectedMod is null ? string.Empty : SelectedMod.ToMarkdownString();

        /// <summary>
        /// Currently selected autoload mod
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedAddonScore))]
        [NotifyPropertyChangedFor(nameof(IsSelectedAddonUpvoted))]
        [NotifyPropertyChangedFor(nameof(IsSelectedAddonDownvoted))]
        [NotifyPropertyChangedFor(nameof(SelectedAddonDescription))]
        private IAddon? _selectedMod;

        public string? SelectedAddonPlaytime => null;

        public int? SelectedAddonScore
        {
            get
            {
                if (SelectedMod is null)
                {
                    return null;
                }

                var hasUpvote = _scoresProvider.GetScore(SelectedMod.Id);

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
                if (SelectedMod is null)
                {
                    return false;
                }

                var hasUpvote = _config.Upvotes.TryGetValue(SelectedMod.Id, out var isUpvote);

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
                if (SelectedMod is null)
                {
                    return false;
                }

                var hasUpvote = _config.Upvotes.TryGetValue(SelectedMod.Id, out var isUpvote);

                if (hasUpvote && !isUpvote)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsPreviewVisible => false;

        public Stream? SelectedAddonPreview => null;

        #endregion


        #region Relay Commands

        /// <summary>
        /// Open mods folder
        /// </summary>
        [RelayCommand]
        private void OpenFolder()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Game.ModsFolderPath,
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
        /// Delete selected mod
        /// </summary>
        [RelayCommand]
        private void DeleteMod()
        {
            SelectedMod.ThrowIfNull();

            Game.InstalledAddonsProvider.DeleteAddon(SelectedMod);

            OnPropertyChanged(nameof(ModsList));
        }


        /// <summary>
        /// Delete selected map/campaign
        /// </summary>
        [RelayCommand]
        private void ModCheckboxPressed(object? obj)
        {
            obj.ThrowIfNotType<AutoloadMod>(out var mod);

            if (!mod.IsEnabled)
            {
                _config.AddDisabledAutoloadMod(mod.Id);
                Game.InstalledAddonsProvider.DisableAddon(mod.Id);
            }
            else if (mod.IsEnabled)
            {
                _config.RemoveDisabledAutoloadMod(mod.Id);
                Game.InstalledAddonsProvider.EnableAddon(mod.Id);
            }
        }


        /// <summary>
        /// Upvote fix
        /// </summary>
        [RelayCommand]
        private async Task Upvote()
        {
            SelectedMod.ThrowIfNull();

            var newScore = await _apiInterface.ChangeVoteAsync(SelectedMod, true).ConfigureAwait(true);
            _scoresProvider.ChangeScore(SelectedMod.Id, newScore);

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
            SelectedMod.ThrowIfNull();

            var newScore = await _apiInterface.ChangeVoteAsync(SelectedMod, false).ConfigureAwait(true);
            _scoresProvider.ChangeScore(SelectedMod.Id, newScore);

            OnPropertyChanged(nameof(SelectedAddonScore));
            OnPropertyChanged(nameof(IsSelectedAddonUpvoted));
            OnPropertyChanged(nameof(IsSelectedAddonDownvoted));
        }

        #endregion


        private void OnGameChanged(GameEnum parameterName)
        {
            if (parameterName == Game.GameEnum)
            {
                OnPropertyChanged(nameof(ModsList));
            }
        }

        private void OnModDownloaded(IGame game, AddonTypeEnum addonType)
        {
            if (game.GameEnum != Game.GameEnum ||
                addonType is not AddonTypeEnum.Mod)
            {
                return;
            }

            OnPropertyChanged(nameof(ModsList));
        }
    }
}
