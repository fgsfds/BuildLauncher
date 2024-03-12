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
                    AddonEnum = RedneckAddonEnum.Redneck,
                    Author = "Xatrix Entertainment",
                    Description = """
                        **Redneck Rampage** is a 1997 first-person shooter game developed by **Xatrix Entertainment** and published by **Interplay**.
                        The game is a first-person shooter with a variety of weapons and levels, and has a hillbilly theme, primarily taking place in a fictional Arkansas town.
                        Many of the weapons and power-ups border on the nonsensical, and in some ways the game is a parody of both first-person shooter games and rural American life.

                        The game's plot revolves around two brothers, Leonard and Bubba, fighting through the fictional town of Hickston, Arkansas to rescue their prized pig Bessie and thwart an alien invasion.
                        The brothers battle through such locales as a meat packing plant and a trailer park, and battle evil clones of their neighbors. There are also male and female alien enemies.
                        The bosses are the Assface and the leader of the alien invasion, the Queen Vixen.
                        """,
                    StartupFile = null,
                    Version = null,
                    SupportedPorts = null,
                    Url = null,
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
                        AddonEnum = RedneckAddonEnum.Route66,
                        Author = "Sunstorm Interactive",
                        Description = """
                            **Redneck Rampage: Suckin' Grits on Route 66** is a 12-level expansion pack for Redneck Rampage. It was developed by Sunstorm Interactive and released on December 19, 1997.
                            The add-on contains several new locations and textures, as well as a new ending.
                            """,
                        StartupFile = null,
                        Version = null,
                        SupportedPorts = null,
                        Url = null,
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
                    AddonEnum = RedneckAddonEnum.Again,
                    Author = "Xatrix Entertainment",
                    Description = """
                        **Redneck Rampage Rides Again** is a sequel to Redneck Rampage developed by **Xatrix Entertainment** and published by **Interplay Entertainment** for MS-DOS in 1998.

                        After Leonard and Bubba crash-land a UFO, they find themselves in the middle of the desert.
                        Along the way, they are hunted by aliens and must blast their way through jackalope farms, Disgraceland, a riverboat, a brothel and various other locales.
                        """,
                    Version = null,
                    StartupFile = null,
                    SupportedPorts = null,
                    Url = null,
                    IsOfficial = true,
                    PathToFile = null
                });
            }

            return campaigns;
        }
    }
}
