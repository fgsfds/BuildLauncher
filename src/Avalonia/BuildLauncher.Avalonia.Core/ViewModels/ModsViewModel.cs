using Common.Config;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Games.Providers;
using Mods.Mods;
using System.Collections.Immutable;
using System.Diagnostics;

namespace BuildLauncher.ViewModels
{
    public sealed partial class ModsViewModel : ObservableObject
    {
        public readonly IGame Game;
        private readonly GamesProvider _gamesProvider;
        private readonly ConfigEntity _config;


        [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
        public ModsViewModel(
            IGame game,
            GamesProvider gamesProvider,
            ConfigEntity config)
        {
            Game = game;

            _gamesProvider = gamesProvider;
            _config = config;

            _gamesProvider.GameChangedEvent += OnGameChanged;
            Game.DownloadableModsProvider.ModDownloadedEvent += OnModDownloaded;
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
            await Game.InstalledModsProvider.CreateCache(createNew);

            OnPropertyChanged(nameof(ModsList));
        }


        #region Binding Properties

        /// <summary>
        /// List of installed autoload mods
        /// </summary>
        public ImmutableList<IAddon> ModsList => Game.GetAutoloadMods(false).Select(x => x.Value).ToImmutableList();

        /// <summary>
        /// Currently selected autoload mod
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedModDescription))]
        private IAddon? _selectedMod;

        public string SelectedModDescription => SelectedMod is null ? string.Empty : SelectedMod.ToMarkdownString();

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

            Game.InstalledModsProvider.DeleteMod(SelectedMod);

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
            }
            else if (mod.IsEnabled)
            {
                _config.RemoveDisabledAutoloadMod(mod.Id);
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

        private void OnModDownloaded(IGame game, ModTypeEnum modType)
        {
            if (game.GameEnum != Game.GameEnum ||
                modType is not ModTypeEnum.Mod)
            {
                return;
            }

            OnPropertyChanged(nameof(ModsList));
        }
    }
}
