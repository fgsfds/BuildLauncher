using Common.Enums;
using Common.Interfaces;

namespace Ports.Ports.EDuke32
{
    /// <summary>
    /// NotBlood port
    /// </summary>
    public sealed class NotBlood : NBlood
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.NotBlood;

        /// <inheritdoc/>
        public override string Exe => "notblood.exe";

        /// <inheritdoc/>
        public override string Name => "NotBlood";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames => [GameEnum.Blood];

        /// <inheritdoc/>
        public override List<FeatureEnum> SupportedFeatures =>
            [
            FeatureEnum.ModernTypes,
            FeatureEnum.Hightile,
            FeatureEnum.Models
            ];

        /// <inheritdoc/>
        protected override string ConfigFile => "notblood.cfg";


        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IAddon campaign)
        {
            //nothing to do
        }
    }
}
