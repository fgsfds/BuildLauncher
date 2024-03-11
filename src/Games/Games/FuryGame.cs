using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Mods.Mods;
using Mods.Providers;

namespace Games.Games
{
    public sealed class FuryGame : BaseGame
    {
        /// <inheritdoc/>
        public override GameEnum GameEnum => GameEnum.Fury;

        /// <inheritdoc/>
        public override string FullName => "Ion Fury";

        /// <inheritdoc/>
        public override string ShortName => "Fury";

        /// <inheritdoc/>
        public override string MainFile => "fury.grp";

        /// <inheritdoc/>
        public override string DefFile => "fury.def";

        /// <inheritdoc/>
        public override List<string> RequiredFiles => [MainFile];


        public FuryGame(InstalledModsProvider modsProvider) : base(modsProvider)
        {
        }


        /// <inheritdoc/>
        protected override List<IMod> GetOriginalCampaigns()
        {
            List<IMod> campaigns = new(1);

            if (IsBaseGameInstalled)
            {
                campaigns.Add(new FuryCampaign()
                {
                    Guid = new(Consts.FuryGuid),
                    ModType = ModTypeEnum.Campaign,
                    DisplayName = "Ion Fury",
                    Image = ImageHelper.FileNameToStream("Fury.fury.jpg"),
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
