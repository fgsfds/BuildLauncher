using Common.Enums;
using Common.Helpers;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Ports.Providers
{
    /// <summary>
    /// Class that provides singleton instances of port types
    /// </summary>
    public sealed class PortsProvider
    {
        private readonly List<BasePort> _ports;

        public BuildGDX BuildGDX { get; init; }
        public EDuke32 EDuke32 { get; init; }
        public NBlood NBlood { get; init; }
        public NotBlood NotBlood { get; init; }
        public PCExhumed PCExhumed { get; init; }
        public Raze Raze { get; init; }
        public RedNukem RedNukem { get; init; }
        public VoidSW VoidSW { get; init; }


        public PortsProvider()
        {
            BuildGDX = new();
            EDuke32 = new();
            NBlood = new();
            NotBlood = new();
            PCExhumed = new();
            Raze = new();
            RedNukem = new();
            VoidSW = new();

            _ports = [BuildGDX, EDuke32, NBlood, NotBlood, PCExhumed, Raze, RedNukem, VoidSW];
        }


        /// <summary>
        /// Get list of ports that support selected game
        /// </summary>
        /// <param name="game">Game enum</param>
        public IEnumerable<BasePort> GetPortsThatSupportGame(GameEnum game) => _ports.Where(x => x.SupportedGames.Contains(game));

        /// <summary>
        /// Get port by enum
        /// </summary>
        /// <param name="portEnum">Port enum</param>
        public BasePort GetPort(PortEnum portEnum)
        {
            return portEnum switch
            {
                PortEnum.BuildGDX => BuildGDX,
                PortEnum.Raze => Raze,
                PortEnum.EDuke32 => EDuke32,
                PortEnum.RedNukem => RedNukem,
                PortEnum.NBlood => NBlood,
                PortEnum.NotBlood => NotBlood,
                PortEnum.VoidSW => VoidSW,
                PortEnum.PCExhumed => PCExhumed,
                _ => ThrowHelper.NotImplementedException<BasePort>()
            };
        }
    }
}
