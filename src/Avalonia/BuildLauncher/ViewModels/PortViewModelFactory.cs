using Common.Enums;
using Ports.Providers;
using Ports.Tools;

namespace BuildLauncher.ViewModels
{
    public sealed class PortViewModelFactory
    {
        private readonly PortsInstallerFactory _installerFactory;
        private readonly PortsProvider _portsProvider;

        public PortViewModelFactory(
            PortsInstallerFactory installerFactory,
            PortsProvider portsProvider
            )
        {
            _installerFactory = installerFactory;
            _portsProvider = portsProvider;
        }

        /// <summary>
        /// Create <see cref="PortViewModel"/>
        /// </summary>
        /// <param name="portEnum">Port enum</param>
        public PortViewModel Create(PortEnum portEnum)
        {
            PortViewModel vm = new(_installerFactory, _portsProvider.GetPort(portEnum));
            vm.InitCommand.Execute(null);
            return vm;
        }
    }
}
