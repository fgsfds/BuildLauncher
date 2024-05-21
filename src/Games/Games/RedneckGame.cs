using ClientCommon.Providers;
using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Mods.Addons;
using Mods.Providers;

namespace Games.Games
{
    public sealed class RedneckGame(
        InstalledAddonsProviderFactory installedModsProviderFactory,
        DownloadableAddonsProviderFactory downloadableModsProviderFactory,
        PlaytimeProvider playtimeProvider
        ) : BaseGame(installedModsProviderFactory, downloadableModsProviderFactory, playtimeProvider)
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
            Dictionary<string, IAddon> campaigns = new(3, StringComparer.OrdinalIgnoreCase);

            if (IsBaseGameInstalled)
            {
                var redneckId = nameof(GameEnum.Redneck).ToLower();
                campaigns.Add(redneckId, new RedneckCampaign()
                {
                    Id = redneckId,
                    Type = AddonTypeEnum.Official,
                    Title = "Redneck Rampage",
                    GridImage = ImageHelper.FileNameToStream("Redneck.redneck.jpg"),
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
                    SupportedGame = GameEnum.Redneck,
                    SupportedPorts = null,
                    PathToFile = null,
                    Dependencies = null,
                    Incompatibles = null,
                    MainCon = null,
                    AdditionalCons = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    RTS = null,
                    StartMap = null,
                    RequiredAddonEnum = RedneckAddonEnum.Redneck,
                    RequiredFeatures = null,
                    PreviewImage = null,
                    Playtime = _playtimeProvider.GetTime(redneckId)
                });

                if (IsRoute66Installed)
                {
                    var redneckR66Id = nameof(RedneckAddonEnum.RedneckR66).ToLower();
                    campaigns.Add(redneckR66Id, new RedneckCampaign()
                    {
                        Id = redneckR66Id,
                        Type = AddonTypeEnum.Official,
                        Title = "Route 66",
                        GridImage = ImageHelper.FileNameToStream("Redneck.route66.jpg"),
                        Author = "Sunstorm Interactive",
                        Description = """
                            **Redneck Rampage: Suckin' Grits on Route 66** is a 12-level expansion pack for Redneck Rampage. It was developed by Sunstorm Interactive and released on December 19, 1997.
                            The add-on contains several new locations and textures, as well as a new ending.
                            """,
                        Version = null,
                        SupportedGame = GameEnum.Redneck,
                        SupportedPorts = null,
                        PathToFile = null,
                        Dependencies = null,
                        Incompatibles = null,
                        MainCon = null,
                        AdditionalCons = null,
                        MainDef = null,
                        AdditionalDefs = null,
                        RTS = null,
                        StartMap = null,
                        RequiredAddonEnum = RedneckAddonEnum.RedneckR66,
                        RequiredFeatures = null,
                        PreviewImage = null,
                        Playtime = _playtimeProvider.GetTime(redneckR66Id)
                    });
                }
            }

            if (IsAgainInstalled)
            {
                var redneckRaId = nameof(GameEnum.RidesAgain).ToLower();
                campaigns.Add(redneckRaId, new RedneckCampaign()
                {
                    Id = redneckRaId,
                    Type = AddonTypeEnum.Official,
                    Title = "Rides Again",
                    GridImage = ImageHelper.FileNameToStream("Redneck.again.jpg"),
                    Author = "Xatrix Entertainment",
                    Description = """
                        **Redneck Rampage Rides Again** is a sequel to Redneck Rampage developed by **Xatrix Entertainment** and published by **Interplay Entertainment** for MS-DOS in 1998.

                        After Leonard and Bubba crash-land a UFO, they find themselves in the middle of the desert.
                        Along the way, they are hunted by aliens and must blast their way through jackalope farms, Disgraceland, a riverboat, a brothel and various other locales.
                        """,
                    Version = null,
                    SupportedGame = GameEnum.RidesAgain,
                    SupportedPorts = null,
                    PathToFile = null,
                    Dependencies = null,
                    Incompatibles = null,
                    MainCon = null,
                    AdditionalCons = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    RTS = null,
                    StartMap = null,
                    RequiredAddonEnum = RedneckAddonEnum.RedneckRA,
                    RequiredFeatures = null,
                    PreviewImage = null,
                    Playtime = _playtimeProvider.GetTime(redneckRaId)
                });
            }

            return campaigns;
        }
    }
}
