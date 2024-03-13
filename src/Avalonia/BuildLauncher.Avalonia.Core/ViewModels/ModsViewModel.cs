using Common.Config;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Games.Providers;
using Mods.Mods;
using Mods.Providers;
using System.Collections.Immutable;
using System.Diagnostics;

namespace BuildLauncher.ViewModels
{
    public sealed partial class ModsViewModel : ObservableObject
    {
        public readonly IGame Game;
        private readonly GamesProvider _gamesProvider;
        private readonly DownloadableModsProvider _downloadableModsProvider;
        private readonly InstalledModsProvider _installedModsProvider;
        private readonly ConfigEntity _config;

        public ModsViewModel(
            IGame game,
            GamesProvider gamesProvider,
            DownloadableModsProvider downloadableModsProvider,
            InstalledModsProvider installedModsProvider,
            ConfigEntity config)
        {
            Game = game;

            _gamesProvider = gamesProvider;
            _downloadableModsProvider = downloadableModsProvider;
            _installedModsProvider = installedModsProvider;
            _config = config;

            _gamesProvider.NotifyGameChanged += NotifyGameChanged;
            _downloadableModsProvider.NotifyModDownloaded += NotifyModDownloaded;
        }


        /// <summary>
        /// VM initialization
        /// </summary>
        public Task InitializeAsync() => UpdateAsync();

        /// <summary>
        /// Update mods list
        /// </summary>
        private async Task UpdateAsync()
        {
            await _installedModsProvider.UpdateCachedListAsync(Game);

            OnPropertyChanged(nameof(ModsList));
        }


        #region Binding Properties

        /// <summary>
        /// List of installed autoload mods
        /// </summary>
        public ImmutableList<AutoloadMod> ModsList => Game.GetAutoloadMods(false).Select(x => (AutoloadMod)x.Value).ToImmutableList();

        /// <summary>
        /// Currently selected autoload mod
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedModDescription))]
        private IMod? _selectedMod;

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
        /// Delete selected mod
        /// </summary>
        [RelayCommand]
        private void DeleteMod()
        {
            SelectedMod.ThrowIfNull();

            _installedModsProvider.DeleteMod(Game, SelectedMod);

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
                _config.AddDisabledAutoloadMod(mod.Guid);
            }
            else if (mod.IsEnabled)
            {
                _config.RemoveDisabledAutoloadMod(mod.Guid);
            }

            Game.CreateCombinedMod();
        }

        #endregion


        private void NotifyGameChanged(GameEnum parameterName)
        {
            if (parameterName == Game.GameEnum)
            {
                OnPropertyChanged(nameof(ModsList));
            }
        }

        private void NotifyModDownloaded(IGame game, ModTypeEnum modType)
        {
            if (game.GameEnum != Game.GameEnum ||
                modType is not ModTypeEnum.Autoload)
            {
                return;
            }

            OnPropertyChanged(nameof(ModsList));
        }
    }
}
