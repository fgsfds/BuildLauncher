using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Mods.Mods;
using Mods.Providers;

namespace Games.Games
{
    public sealed class RedneckGame(InstalledModsProviderFactory modsProvider, DownloadableModsProviderFactory downloadableModsProviderFactory) : BaseGame(modsProvider, downloadableModsProviderFactory)
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
        public required string? AgainInstallPath { get; set; }

        /// <inheritdoc/>
        public override string DefFileName => "redneck.def";

        /// <inheritdoc/>
        public override List<string> RequiredFiles => ["REDNECK.GRP"];

        /// <summary>
        /// Is Route 66 installed
        /// </summary>
        public bool IsRoute66Installed => IsInstalled(["TILESA66.ART", "TILESB66.ART", "TURD66.ANM", "TURD66.VOC", "END66.ANM", "END66.VOC", "BUBBA66.CON", "DEFS66.CON", "GATOR66.CON", "GAME66.CON", "PIG66.CON"]);

        /// <summary>
        /// Is Rides Again installed
        /// </summary>
        public bool IsAgainInstalled => IsInstalled(["REDNECK.GRP", "BIKER.CON"], AgainInstallPath);


        /// <inheritdoc/>
        protected override Dictionary<string, IAddon> GetOriginalCampaigns()
        {
            Dictionary<string, IAddon> campaigns = new(3);

            if (IsBaseGameInstalled)
            {
                campaigns.Add(GameEnum.Redneck.ToString(), new RedneckCampaign()
                {
                    Id = GameEnum.Redneck.ToString(),
                    Type = ModTypeEnum.Official,
                    Title = "Redneck Rampage",
                    Image = ImageHelper.FileNameToStream("Redneck.redneck.jpg"),
                    Author = "Xatrix Entertainment",
                    Description = """
                        **Redneck Rampage** is a 1997 first-person shooter game developed by **Xatrix Entertainment** and published by **Interplay**.
                        The game is a first-person shooter with a variety of weapons and levels, and has a hillbilly theme, primarily taking place in a fictional Arkansas town.
                        Many of the weapons and power-ups border on the nonsensical, and in some ways the game is a parody of both first-person shooter games and rural American life.

                        The game's plot revolves around two brothers, Leonard and Bubba, fighting through the fictional town of Hickston, Arkansas to rescue their prized pig Bessie and thwart an alien invasion.
                        The brothers battle through such locales as a meat packing plant and a trailer park, and battle evil clones of their neighbors. There are also male and female alien enemies.
                        The bosses are the Assface and the leader of the alien invasion, the Queen Vixen.
                        """,
                    Version = null,
                    SupportedPorts = null,
                    PathToFile = null,
                    SupportedGames = null,
                    SupportedGamesCrcs = null,
                    Dependencies = null,
                    Incompatibles = null,
                    MainCon = null,
                    AdditionalCons = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    RTS = null,
                    StartMap = null,
                    RequiredAddonEnum = RedneckAddonEnum.Redneck
                });

                if (IsRoute66Installed)
                {
                    campaigns.Add(RedneckAddonEnum.RedneckR66.ToString(), new RedneckCampaign()
                    {
                        Id = RedneckAddonEnum.RedneckR66.ToString(),
                        Type = ModTypeEnum.Official,
                        Title = "Route 66",
                        Image = ImageHelper.FileNameToStream("Redneck.route66.jpg"),
                        Author = "Sunstorm Interactive",
                        Description = """
                            **Redneck Rampage: Suckin' Grits on Route 66** is a 12-level expansion pack for Redneck Rampage. It was developed by Sunstorm Interactive and released on December 19, 1997.
                            The add-on contains several new locations and textures, as well as a new ending.
                            """,
                        Version = null,
                        SupportedPorts = null,
                        PathToFile = null,
                        SupportedGames = null,
                        SupportedGamesCrcs = null,
                        Dependencies = null,
                        Incompatibles = null,
                        MainCon = null,
                        AdditionalCons = null,
                        MainDef = null,
                        AdditionalDefs = null,
                        RTS = null,
                        StartMap = null,
                        RequiredAddonEnum = RedneckAddonEnum.RedneckR66
                    });
                }
            }

            if (IsAgainInstalled)
            {
                campaigns.Add(GameEnum.RedneckRA.ToString(), new RedneckCampaign()
                {
                    Id = GameEnum.RedneckRA.ToString(),
                    Type = ModTypeEnum.Official,
                    Title = "Rides Again",
                    Image = ImageHelper.FileNameToStream("Redneck.again.jpg"),
                    Author = "Xatrix Entertainment",
                    Description = """
                        **Redneck Rampage Rides Again** is a sequel to Redneck Rampage developed by **Xatrix Entertainment** and published by **Interplay Entertainment** for MS-DOS in 1998.

                        After Leonard and Bubba crash-land a UFO, they find themselves in the middle of the desert.
                        Along the way, they are hunted by aliens and must blast their way through jackalope farms, Disgraceland, a riverboat, a brothel and various other locales.
                        """,
                    Version = null,
                    SupportedPorts = null,
                    PathToFile = null,
                    SupportedGames = null,
                    SupportedGamesCrcs = null,
                    Dependencies = null,
                    Incompatibles = null,
                    MainCon = null,
                    AdditionalCons = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    RTS = null,
                    StartMap = null,
                    RequiredAddonEnum = RedneckAddonEnum.RedneckRA
                });
            }

            return campaigns;
        }
    }
}
