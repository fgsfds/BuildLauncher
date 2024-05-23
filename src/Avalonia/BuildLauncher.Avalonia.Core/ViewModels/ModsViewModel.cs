using ClientCommon.API;
using ClientCommon.Config;
using ClientCommon.Providers;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Games.Providers;
using Mods.Addons;
using System.Collections.Immutable;
using System.Diagnostics;

namespace BuildLauncher.ViewModels
{
    public sealed partial class ModsViewModel : RightPanelViewModel, IPortsButtonControl
    {
        public readonly IGame Game;

        private readonly GamesProvider _gamesProvider;
        private readonly ConfigEntity _config;
        private readonly PlaytimeProvider _playtimeProvider;


        [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
        public ModsViewModel(
            IGame game,
            GamesProvider gamesProvider,
            ConfigEntity config,
            PlaytimeProvider playtimeProvider,
            ApiInterface apiInterface,
            ScoresProvider scoresProvider
            ) : base(config, playtimeProvider, apiInterface, scoresProvider)
        {
            Game = game;

            _gamesProvider = gamesProvider;
            _config = config;
            _playtimeProvider = playtimeProvider;

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

        private IAddon? _selectedAddon;
        /// <summary>
        /// Currently selected autoload mod
        /// </summary>
        public override IAddon? SelectedAddon
        {
            get => _selectedAddon;
            set
            {
                _selectedAddon = value;

                OnPropertyChanged(nameof(SelectedAddonDescription));
                OnPropertyChanged(nameof(SelectedAddonScore));
                OnPropertyChanged(nameof(IsSelectedAddonUpvoted));
                OnPropertyChanged(nameof(IsSelectedAddonDownvoted));
            }
        }

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
            SelectedAddon.ThrowIfNull();

            Game.InstalledAddonsProvider.DeleteAddon(SelectedAddon);

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
