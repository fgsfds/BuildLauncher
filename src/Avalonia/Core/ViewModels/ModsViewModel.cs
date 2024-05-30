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
        private readonly ConfigProvider _config;
        private readonly PlaytimeProvider _playtimeProvider;


        [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
        public ModsViewModel(
            IGame game,
            GamesProvider gamesProvider,
            ConfigProvider config,
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
            Game.DownloadableAddonsProvider.AddonDownloadedEvent += OnAddonChanged;
            Game.InstalledAddonsProvider.AddonsChangedEvent += OnAddonChanged;
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

        public bool IsPortsButtonsVisible => false;

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
        /// Enable/disable mod
        /// </summary>
        [RelayCommand]
        private void ModCheckboxPressed(object? obj)
        {
            obj.ThrowIfNotType<AutoloadMod>(out var mod);

            if (!mod.IsEnabled)
            {
                Game.InstalledAddonsProvider.DisableAddon(mod.Id);
            }
            else if (mod.IsEnabled)
            {
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

        private void OnAddonChanged(IGame game, AddonTypeEnum? addonType)
        {
            if (game.GameEnum == Game.GameEnum && (addonType is AddonTypeEnum.Mod || addonType is null))
            {
                OnPropertyChanged(nameof(ModsList));
            }
        }
    }
}
