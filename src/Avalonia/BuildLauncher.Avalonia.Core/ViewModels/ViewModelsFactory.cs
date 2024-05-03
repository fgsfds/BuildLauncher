﻿using Common.Config;
using Common.Enums;
using Common.Helpers;
using Common.Providers;
using Games.Providers;
using Ports.Installer;
using Ports.Providers;
using Tools.Installer;
using Tools.Tools;

namespace BuildLauncher.ViewModels
{
    public sealed class ViewModelsFactory
    {
        private readonly GamesProvider _gamesProvider;
        private readonly ConfigEntity _config;
        private readonly PortsInstallerFactory _portsInstallerFactory;
        private readonly ToolsInstallerFactory _toolsInstallerFactory;
        private readonly PortsProvider _portsProvider;
        private readonly PlaytimeProvider _playtimeProvider;

        public ViewModelsFactory(
            GamesProvider gamesProvider,
            ConfigProvider configProvider,
            PortsInstallerFactory portsInstallerFactory,
            ToolsInstallerFactory toolsInstallerFactory,
            PortsProvider portsProvider,
            PlaytimeProvider playtimeProvider
            )
        {
            _gamesProvider = gamesProvider;
            _config = configProvider.Config;
            _portsInstallerFactory = portsInstallerFactory;
            _toolsInstallerFactory = toolsInstallerFactory;
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
                _portsInstallerFactory,
                _portsProvider.GetPort(portEnum)
                );

            Task.Run(vm.InitializeAsync);
            return vm;
        }


        /// <summary>
        /// Create <see cref="ToolViewModel"/>
        /// </summary>
        public ToolViewModel GetToolViewModel(string toolName)
        {
            BaseTool tool;

            if (toolName.Equals(nameof(XMapEdit)))
            {
                tool = new XMapEdit(_gamesProvider);
            }
            else if (toolName.Equals(nameof(Mapster32)))
            {
                tool = new Mapster32(_gamesProvider);
            }
            else
            {
                ThrowHelper.ArgumentOutOfRangeException(nameof(toolName));
                return null;
            }

            ToolViewModel vm = new(
                _toolsInstallerFactory,
                _gamesProvider,
                tool
                );

            Task.Run(vm.InitializeAsync);
            return vm;
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
