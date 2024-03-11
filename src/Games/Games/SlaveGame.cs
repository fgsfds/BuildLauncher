using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Mods.Mods;
using Mods.Providers;

namespace Games.Games
{
    public sealed class SlaveGame : BaseGame
    {
        /// <inheritdoc/>
        public override GameEnum GameEnum => GameEnum.Slave;

        /// <inheritdoc/>
        public override string FullName => "Powerslave";

        /// <inheritdoc/>
        public override string ShortName => "Slave";

        /// <inheritdoc/>
        public override string MainFile => "STUFF.DAT";

        /// <inheritdoc/>
        public override string DefFile => "exhumed.def";

        /// <inheritdoc/>
        public override List<string> RequiredFiles => [MainFile];


        public SlaveGame(InstalledModsProvider modsProvider) : base(modsProvider)
        {
        }


        /// <inheritdoc/>
        protected override List<IMod> GetOriginalCampaigns()
        {
            List<IMod> campaigns = new(1);

            if (IsBaseGameInstalled)
            {
                campaigns.Add(new SlaveCampaign()
                {
                    Guid = new(Consts.SlaveGuid),
                    ModType = ModTypeEnum.Campaign,
                    DisplayName = "Powerslave",
                    Image = ImageHelper.FileNameToStream("Slave.slave.jpg"),
                    StartupFile = null,
                    Version = null,
                    SupportedPorts = null,
                    Description = null,
                    Url = null,
                    Author = null,
                    IsOfficial = true,
                    PathToFile = null
                });
            }

            return campaigns;
        }
    }
}
