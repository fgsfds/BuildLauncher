using Common.Enums;
using Common.Helpers;
using Common.Interfaces;

namespace Ports.Ports.EDuke32
{
    /// <summary>
    /// VoidSW port
    /// </summary>
    public sealed class VoidSW : EDuke32
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.VoidSW;

        /// <inheritdoc/>
        public override string Exe => "voidsw.exe";

        /// <inheritdoc/>
        public override string Name => "VoidSW";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames => [GameEnum.Wang];

        /// <inheritdoc/>
        public override string PathToPortFolder => Path.Combine(CommonProperties.PortsFolderPath, "EDuke32");

        /// <inheritdoc/>
        protected override string ConfigFile => "voidsw.cfg";

        /// <inheritdoc/>
        protected override string AddFileParam => "-g";

        /// <inheritdoc/>
        protected override string MainDefParam => "-h";

        /// <inheritdoc/>
        protected override string AddDefParam => "-mh";

        /// <inheritdoc/>
        protected override string AddConParam => ThrowHelper.NotImplementedException<string>();

        /// <inheritdoc/>
        protected override string MainConParam => ThrowHelper.NotImplementedException<string>();


        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IAddon campaign)
        {
            FixGrpInConfig();
        }
    }
}
