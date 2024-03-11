using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Mods.Mods;
using Mods.Providers;

namespace Games.Games
{
    public sealed class RedneckGame : BaseGame
    {
        /// <inheritdoc/>
        public override GameEnum GameEnum => GameEnum.Redneck;

        /// <inheritdoc/>
        public override string FullName => "Redneck Rampage";

        /// <inheritdoc/>
        public override string ShortName => "Redneck";

        /// <summary>
        /// Path to Rides Again folder
        /// </summary>
        public required string AgainInstallPath { get; set; }

        /// <inheritdoc/>
        public override string MainFile => "REDNECK.GRP";

        /// <inheritdoc/>
        public override string DefFile => "redneck.def";

        /// <summary>
        /// List of files required for Route 66
        /// </summary>
        public List<string> RequiredFilesRoute66 => ["END66.ANM"];

        /// <inheritdoc/>
        public override List<string> RequiredFiles => [MainFile];

        /// <summary>
        /// Is Route 66 installed
        /// </summary>
        public bool IsRoute66Installed => IsInstalled(RequiredFilesRoute66);

        /// <summary>
        /// Is Rides Again installed
        /// </summary>
        public bool IsAgainInstalled => IsInstalled([MainFile], AgainInstallPath);


        public RedneckGame(InstalledModsProvider modsProvider) : base(modsProvider)
        {
        }


        /// <inheritdoc/>
        protected override List<IMod> GetOriginalCampaigns()
        {
            List<IMod> campaigns = new(3);

            if (IsBaseGameInstalled)
            {
                campaigns.Add(new RedneckCampaign()
                {
                    Guid = new(Consts.RedneckGuid),
                    ModType = ModTypeEnum.Campaign,
                    DisplayName = "Redneck Rampage",
                    Image = ImageHelper.FileNameToStream("Redneck.redneck.jpg"),
                    StartupFile = null,
                    AddonEnum = RedneckAddonEnum.Redneck,
                    Version = null,
                    SupportedPorts = null,
                    Description = null,
                    Url = null,
                    Author = null,
                    IsOfficial = true,
                    PathToFile = null
                });

                if (IsRoute66Installed)
                {
                    campaigns.Add(new RedneckCampaign()
                    {
                        Guid = new(Consts.Route66Guid),
                        ModType = ModTypeEnum.Campaign,
                        DisplayName = "Route 66",
                        Image = ImageHelper.FileNameToStream("Redneck.route66.jpg"),
                        StartupFile = null,
                        AddonEnum = RedneckAddonEnum.Route66,
                        Version = null,
                        SupportedPorts = [PortEnum.Raze, PortEnum.BuildGDX],
                        Description = null,
                        Url = null,
                        Author = null,
                        IsOfficial = true,
                        PathToFile = null
                    });
                }
            }

            if (IsAgainInstalled)
            {
                campaigns.Add(new RedneckCampaign()
                {
                    Guid = new(Consts.AgainGuid),
                    ModType = ModTypeEnum.Campaign,
                    DisplayName = "Rides Again",
                    Image = ImageHelper.FileNameToStream("Redneck.again.jpg"),
                    StartupFile = null,
                    AddonEnum = RedneckAddonEnum.Again,
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
