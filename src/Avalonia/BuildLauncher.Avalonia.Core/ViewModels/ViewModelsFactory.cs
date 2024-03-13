using Common.Config;
using Common.Enums;
using Games.Providers;
using Mods.Providers;
using Ports.Providers;
using Ports.Tools;

namespace BuildLauncher.ViewModels
{
    public sealed class ViewModelsFactory
    {
        private readonly GamesProvider _gamesProvider;
        private readonly DownloadableModsProvider _modsProvider;
        private readonly ConfigEntity _config;
        private readonly PortsInstallerFactory _installerFactory;
        private readonly PortsProvider _portsProvider;

        public ViewModelsFactory(
            GamesProvider gamesProvider,
            DownloadableModsProvider modsProvider,
            ConfigProvider configProvider,
            PortsInstallerFactory installerFactory,
            PortsProvider portsProvider
            )
        {
            _gamesProvider = gamesProvider;
            _modsProvider = modsProvider;
            _config = configProvider.Config;
            _installerFactory = installerFactory;
            _portsProvider = portsProvider;
        }

        /// <summary>
        /// Create <see cref="CampaignsViewModel"/>
        /// </summary>
        /// <param name="gameEnum">Game enum</param>
        public CampaignsViewModel GetCampaignsViewModel(GameEnum gameEnum)
        {
            CampaignsViewModel vm = new(
                _gamesProvider.GetGame(gameEnum),
                _gamesProvider,
                _modsProvider,
                _config
                );

            return vm;
        }


        /// <summary>
        /// Create <see cref="CampaignsViewModel"/>
        /// </summary>
        /// <param name="gameEnum">Game enum</param>
        public MapsViewModel GetMapsViewModel(GameEnum gameEnum)
        {
            MapsViewModel vm = new(
                _gamesProvider.GetGame(gameEnum),
                _gamesProvider,
                _modsProvider,
                _config
                );

            return vm;
        }

        /// <summary>
        /// Create <see cref="CampaignsViewModel"/>
        /// </summary>
        /// <param name="gameEnum">Game enum</param>
        public ModsViewModel GetModsViewModel(GameEnum gameEnum)
        {
            ModsViewModel vm = new(
                _gamesProvider.GetGame(gameEnum),
                _gamesProvider,
                _modsProvider,
                _config
                );

            return vm;
        }

        /// <summary>
        /// Create <see cref="CampaignsViewModel"/>
        /// </summary>
        /// <param name="gameEnum">Game enum</param>
        public DownloadsViewModel GetDownloadsViewModel(GameEnum gameEnum)
        {
            DownloadsViewModel vm = new(
                _gamesProvider.GetGame(gameEnum),
                _modsProvider
                );

            Task.Run(vm.InitializeAsync);
            return vm;
        }


        /// <summary>
        /// Create <see cref="PortViewModel"/>
        /// </summary>
        /// <param name="portEnum">Port enum</param>
        public PortViewModel GetPortViewModel(PortEnum portEnum)
        {
            PortViewModel vm = new(
                _installerFactory,
                _portsProvider.GetPort(portEnum)
                );

            Task.Run(vm.InitializeAsync);
            return vm;
        }
    }
}
