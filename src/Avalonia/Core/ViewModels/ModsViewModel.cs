using ClientCommon.API;
using ClientCommon.Config;
using ClientCommon.Providers;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Games.Providers;
using Mods.Addons;
using Mods.Providers;
using System.Collections.Immutable;
using System.Diagnostics;

namespace BuildLauncher.ViewModels
{
    public sealed partial class ModsViewModel : RightPanelViewModel, IPortsButtonControl
    {
        public readonly IGame Game;

        private readonly GamesProvider _gamesProvider;
        private readonly IConfigProvider _config;
        private readonly PlaytimeProvider _playtimeProvider;
        private readonly InstalledAddonsProvider _installedAddonsProvider;
        private readonly DownloadableAddonsProvider _downloadableAddonsProvider;


        [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
        public ModsViewModel(
            IGame game,
            GamesProvider gamesProvider,
            IConfigProvider config,
            PlaytimeProvider playtimeProvider,
            ApiInterface apiInterface,
            ScoresProvider scoresProvider,
            InstalledAddonsProviderFactory installedAddonsProviderFactory,
            DownloadableAddonsProviderFactory _downloadableAddonsProviderFactory
            ) : base(config, playtimeProvider, apiInterface, scoresProvider)
        {
            Game = game;

            _gamesProvider = gamesProvider;
            _config = config;
            _playtimeProvider = playtimeProvider;
            _installedAddonsProvider = installedAddonsProviderFactory.GetSingleton(game);
            _downloadableAddonsProvider = _downloadableAddonsProviderFactory.GetSingleton(game);

            _gamesProvider.GameChangedEvent += OnGameChanged;
            _installedAddonsProvider.AddonsChangedEvent += OnAddonChanged;
            _downloadableAddonsProvider.AddonDownloadedEvent += OnAddonChanged;
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
            await _installedAddonsProvider.CreateCache(createNew);

            OnPropertyChanged(nameof(ModsList));
        }


        #region Binding Properties

        /// <summary>
        /// List of installed autoload mods
        /// </summary>
        public ImmutableList<AutoloadMod> ModsList => [.. _installedAddonsProvider.GetInstalledMods().Select(x => (AutoloadMod)x.Value).OrderBy(static x => x.Title)];

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

            _installedAddonsProvider.DeleteAddon(SelectedAddon);

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
                _installedAddonsProvider.DisableAddon(new(mod.Id, mod.Version));
            }
            else if (mod.IsEnabled)
            {
                _installedAddonsProvider.EnableAddon(new(mod.Id, mod.Version));
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
