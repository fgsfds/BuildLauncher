using Common.Config;
using Common.Enums;
using Common.Providers;
using Games.Providers;
using Ports.Providers;
using Ports.Tools;

namespace BuildLauncher.ViewModels
{
    public sealed class ViewModelsFactory
    {
        private readonly GamesProvider _gamesProvider;
        private readonly ConfigEntity _config;
        private readonly PortsInstallerFactory _installerFactory;
        private readonly PortsProvider _portsProvider;
        private readonly PlaytimeProvider _playtimeProvider;

        public ViewModelsFactory(
            GamesProvider gamesProvider,
            ConfigProvider configProvider,
            PortsInstallerFactory installerFactory,
            PortsProvider portsProvider,
            PlaytimeProvider playtimeProvider
            )
        {
            _gamesProvider = gamesProvider;
            _config = configProvider.Config;
            _installerFactory = installerFactory;
            _portsProvider = portsProvider;
            _playtimeProvider = playtimeProvider;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        /// <summary>
        /// Create <see cref="CampaignsViewModel"/>
        /// </summary>
        /// <param name="gameEnum">Game enum</param>
        public CampaignsViewModel GetCampaignsViewModel(GameEnum gameEnum)
        {
            CampaignsViewModel vm = new(
                _gamesProvider.GetGame(gameEnum),
                _gamesProvider,
                _config,
                _playtimeProvider
                );

            Task.Run(vm.InitializeAsync);
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
                _config,
                _playtimeProvider
                );

            Task.Run(vm.InitializeAsync);
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
                _config
                );

            Task.Run(vm.InitializeAsync);
            return vm;
        }

        /// <summary>
        /// Create <see cref="CampaignsViewModel"/>
        /// </summary>
        /// <param name="gameEnum">Game enum</param>
        public DownloadsViewModel GetDownloadsViewModel(GameEnum gameEnum)
        {
            DownloadsViewModel vm = new(
                _gamesProvider.GetGame(gameEnum)
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
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
