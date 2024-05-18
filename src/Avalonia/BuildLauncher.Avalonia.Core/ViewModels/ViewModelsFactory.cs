using ClientCommon.Config;
using ClientCommon.Providers;
using Common.Enums;
using Games.Providers;
using Ports.Installer;
using Ports.Providers;
using Tools.Installer;
using Tools.Providers;

namespace BuildLauncher.ViewModels
{
    public sealed class ViewModelsFactory
    {
        private readonly GamesProvider _gamesProvider;
        private readonly ConfigEntity _config;
        private readonly PortsInstallerFactory _portsInstallerFactory;
        private readonly ToolsInstallerFactory _toolsInstallerFactory;
        private readonly PortsProvider _portsProvider;
        private readonly ToolsProvider _toolsProvider;
        private readonly PlaytimeProvider _playtimeProvider;
        private readonly PortsReleasesProvider _portsReleasesProvider;
        private readonly ToolsReleasesProvider _toolsReleasesProvider;

        public ViewModelsFactory(
            GamesProvider gamesProvider,
            ConfigProvider configProvider,
            PortsInstallerFactory portsInstallerFactory,
            ToolsInstallerFactory toolsInstallerFactory,
            PortsProvider portsProvider,
            ToolsProvider toolsProvider,
            PlaytimeProvider playtimeProvider,
            PortsReleasesProvider portsReleasesProvider,
            ToolsReleasesProvider toolsReleasesProvider
            )
        {
            _gamesProvider = gamesProvider;
            _config = configProvider.Config;
            _portsInstallerFactory = portsInstallerFactory;
            _toolsInstallerFactory = toolsInstallerFactory;
            _portsProvider = portsProvider;
            _toolsProvider = toolsProvider;
            _playtimeProvider = playtimeProvider;
            _portsReleasesProvider = portsReleasesProvider;
            _toolsReleasesProvider = toolsReleasesProvider;
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
                _portsInstallerFactory,
                _portsReleasesProvider,
                _portsProvider.GetPort(portEnum)
                );

            Task.Run(vm.InitializeAsync);
            return vm;
        }


        /// <summary>
        /// Create <see cref="ToolViewModel"/>
        /// </summary>
        /// <param name="toolEnum">Tool enum</param>
        public ToolViewModel GetToolViewModel(ToolEnum toolEnum)
        {
            ToolViewModel vm = new(
                _toolsInstallerFactory,
                _toolsReleasesProvider,
                _gamesProvider,
                _toolsProvider.GetTool(toolEnum)
                );

            Task.Run(vm.InitializeAsync);
            return vm;
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
