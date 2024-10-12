using Addons.Addons;
using Common;
using Common.Client.Helpers;
using Common.Enums;
using Common.Enums.Addons;
using Common.Enums.Versions;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Diagnostics;
using Games.Games;
using System.Reflection;

namespace Addons.Providers;

public sealed class OriginalCampaignsProvider
{
    private readonly IGame _game;

    public OriginalCampaignsProvider(IGame game)
    {
        _game = game;
    }

    /// <inheritdoc/>
    public Dictionary<AddonVersion, IAddon> GetOriginalCampaigns()
    {
        return _game.GameEnum switch
        {
            GameEnum.Duke3D => GetDuke3DCampaigns(),
            GameEnum.Blood => GetBloodCampaigns(),
            GameEnum.ShadowWarrior => GetWangCampaigns(),
            GameEnum.Fury => GetFuryCampaigns(),
            GameEnum.Exhumed => GetSlaveCampaigns(),
            GameEnum.NAM => GetNamCampaigns(),
            GameEnum.WW2GI => GetWw2Campaigns(),
            GameEnum.Redneck => GetRedneckCampaigns(),
            GameEnum.TekWar => GetTekWarCampaigns(),
            GameEnum.Witchaven => GetWitchavenCampaigns(),

            GameEnum.Standalone => [],

            GameEnum.Duke64 => ThrowHelper.ThrowNotSupportedException<Dictionary<AddonVersion, IAddon>>(),
            GameEnum.RidesAgain => ThrowHelper.ThrowNotSupportedException<Dictionary<AddonVersion, IAddon>>(),
            GameEnum.Witchaven2 => ThrowHelper.ThrowNotSupportedException<Dictionary<AddonVersion, IAddon>>(),

            _ => ThrowHelper.ThrowArgumentOutOfRangeException<Dictionary<AddonVersion, IAddon>>()
        };
    }

    private Dictionary<AddonVersion, IAddon> GetDuke3DCampaigns()
    {
        Guard2.ThrowIfNotType<DukeGame>(_game, out var dGame);

        Dictionary<AddonVersion, IAddon> campaigns = new(6);

        if (dGame.IsWorldTourInstalled)
        {
            var dukeWtId = nameof(DukeVersionEnum.Duke3D_WT).ToLower();
            campaigns.Add(new(dukeWtId), new DukeCampaign()
            {
                Id = dukeWtId,
                Type = AddonTypeEnum.Official,
                Title = "Duke Nukem 3D World Tour",
                GridImage = ImageHelper.FileNameToStream("Duke3D.dukewt.jpg", Assembly.GetExecutingAssembly()),
                Author = "Nerve Software, Gearbox Software",
                Description = """
                    **Duke Nukem 3D: 20th Anniversary World Tour** is a 2016 special edition of Duke Nukem 3D.
                    This edition includes all content from Duke Nukem 3D: Atomic Edition, but it adds new levels, enemies, a weapon, and several special features.

                    The 20th Anniversary Edition includes a new fifth episode known as Alien World Order.
                    The episode was designed by Allen Blum and Richard “Levelord” Gray, both of whom designed all the levels in the original Duke Nukem 3D. 
                    """,
                Version = null,
                SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_WT),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
                MainDef = null,
                AdditionalDefs = null,
                RTS = null,
                StartMap = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            });
        }

        if (dGame.IsBaseGameInstalled)
        {
            if (dGame.GameInstallFolder != dGame.DukeWTInstallPath)
            {
                var dukeId = nameof(GameEnum.Duke3D).ToLower();
                campaigns.Add(new(dukeId), new DukeCampaign()
                {
                    Id = dukeId,
                    Type = AddonTypeEnum.Official,
                    Title = "Duke Nukem 3D",
                    GridImage = ImageHelper.FileNameToStream("Duke3D.duke3d.jpg", Assembly.GetExecutingAssembly()),
                    Author = "3D Realms",
                    Description = """
                    Duke Nukem 3D is a first-person shooter developed and published by **3D Realms**.
                    Released on April 19, 1996, Duke Nukem 3D is the third game in the Duke Nukem series and a sequel to Duke Nukem II.

                    The player assumes the role of Duke Nukem, an imperious action hero, and fights through 48 levels spread across 5 episodes. The player encounters a host of enemies and fights them with a range of weaponry.
                    In the end, Duke annihilates the alien overlords and celebrates by desecrating their corpses.
                    """,
                    Version = null,
                    SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
                    RequiredFeatures = null,
                    PathToFile = null,
                    DependentAddons = null,
                    IncompatibleAddons = null,
                    MainCon = null,
                    AdditionalCons = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    RTS = null,
                    StartMap = null,
                    PreviewImage = null,
                    IsFolder = false,
                    Executables = null
                });
            }

            if (dGame.IsCaribbeanInstalled)
            {
                var dukeVacaId = nameof(DukeAddonEnum.DukeVaca).ToLower();
                campaigns.Add(new(dukeVacaId), new DukeCampaign()
                {
                    Id = dukeVacaId,
                    Type = AddonTypeEnum.Official,
                    Title = "Caribbean",
                    GridImage = ImageHelper.FileNameToStream("Duke3D.carib.jpg", Assembly.GetExecutingAssembly()),
                    Author = "Sunstorm Interactive",
                    Description = """
                        **Life's A Beach** is an expansion pack for the highly acclaimed first-person shooter Duke Nukem 3D. It was released on December 31, 1997 by **Sunstorm Interactive**.

                        Narrative elements in Duke Caribbean: Life's A Beach are sparse. According to the official game manual, Duke Nukem is on vacation in the Caribbean to take a break from killing aliens.
                        However, the aliens have decided that the Caribbean offers the perfect climate for a new breeding ground, so they begin laying eggs and terrorizing the local tourists.
                        Angered that his rest and relaxation is being delayed, Duke Nukem sets out on a mission for retribution against the aliens who are interrupting his vacation.
                        """,
                    Version = null,
                    SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
                    RequiredFeatures = null,
                    PathToFile = null,
                    DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { nameof(DukeAddonEnum.DukeVaca), null } },
                    IncompatibleAddons = null,
                    MainCon = null,
                    AdditionalCons = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    RTS = null,
                    StartMap = null,
                    PreviewImage = null,
                    IsFolder = false,
                    Executables = null
                });
            }

            if (dGame.IsNuclearWinterInstalled)
            {
                var dukeNwId = nameof(DukeAddonEnum.DukeNW).ToLower();
                campaigns.Add(new(dukeNwId), new DukeCampaign()
                {
                    Id = dukeNwId,
                    Type = AddonTypeEnum.Official,
                    Title = "Nuclear Winter",
                    GridImage = ImageHelper.FileNameToStream("Duke3D.nwinter.jpg", Assembly.GetExecutingAssembly()),
                    Author = "Simply Silly Software",
                    Description = """
                        **Nuclear Winter**, is a Christmas-themed expansion pack for Duke Nukem 3D. It was developed by **Simply Silly Software** and published by **WizardWorks** on December 30, 1997.

                        Santa Claus has been captured and brainwashed by the aliens that Duke previously defeated. To make matters worse, the aliens are now supported by an enemy force calling themselves the Feminist Elven Militia.
                        Duke Nukem must travel to the North Pole in order to stop the brainwashed Santa Claus and his manipulative captors.
                        """,
                    Version = null,
                    SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
                    RequiredFeatures = null,
                    PathToFile = null,
                    DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { nameof(DukeAddonEnum.DukeNW), null } },
                    IncompatibleAddons = null,
                    MainCon = null,
                    AdditionalCons = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    RTS = null,
                    StartMap = null,
                    PreviewImage = null,
                    IsFolder = false,
                    Executables = null
                });
            }

            if (dGame.IsDukeDCInstalled)
            {
                var dukeDcId = nameof(DukeAddonEnum.DukeDC).ToLower();
                campaigns.Add(new(dukeDcId), new DukeCampaign()
                {
                    Id = dukeDcId,
                    Type = AddonTypeEnum.Official,
                    Title = "Duke it Out in DC",
                    GridImage = ImageHelper.FileNameToStream("Duke3D.dukedc.jpg", Assembly.GetExecutingAssembly()),
                    Author = "WizardWorks",
                    Description = """
                        **Duke It Out In D.C.** is a Duke Nukem 3D expansion pack developed by Sunstorm Interactive and published by **WizardWorks** on March 17, 1997.
                        The add-on does not introduce any new enemies, weapons, or sprites, but it features an all-new episode comprised of ten original levels,
                        each based on a famous location in Washington, D.C. 

                        Aliens have crash-landed into the Capitol Building and have launched a massive invasion of Washington, D.C. Duke Nukem arrives to find that the
                        alien invaders have captured several national monuments and critical government buildings, but in the end, Duke defeats the invading army and rescues the President from the Cycloid Emperor.
                        """,
                    Version = null,
                    SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
                    RequiredFeatures = null,
                    PathToFile = null,
                    DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { nameof(DukeAddonEnum.DukeDC), null } },
                    IncompatibleAddons = null,
                    MainCon = null,
                    AdditionalCons = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    RTS = null,
                    StartMap = null,
                    PreviewImage = null,
                    IsFolder = false,
                    Executables = null
                });
            }
        }

        if (dGame.IsDuke64Installed)
        {
            var duke64Id = nameof(GameEnum.Duke64).ToLower();
            campaigns.Add(new(duke64Id), new DukeCampaign()
            {
                Id = duke64Id,
                Type = AddonTypeEnum.Official,
                Title = "Duke Nukem 64",
                GridImage = ImageHelper.FileNameToStream("Duke3D.duke64.jpg", Assembly.GetExecutingAssembly()),
                Author = "3D Realms, Eurocom",
                Description = """
                    **Duke Nukem 64** is the Nintendo 64 port of the first-person shooter MS-DOS/PC game Duke Nukem 3D.
                    The Nintendo 64 port features significant changes from the PC version, including modified and expanded levels and a different set of weapons.
                    The port also includes a four-player deathmatch mode and a two-player co-op mode via split-screen.
                    The game's mature themes have been minimized to satisfy Nintendo's adult content standards.
                    """,
                Version = null,
                SupportedGame = new(GameEnum.Duke64),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
                MainDef = null,
                AdditionalDefs = null,
                RTS = null,
                StartMap = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            });
        }

        return campaigns;
    }


    /// <inheritdoc/>
    private Dictionary<AddonVersion, IAddon> GetBloodCampaigns()
    {
        Guard2.ThrowIfNotType<BloodGame>(_game, out BloodGame? bGame);

        Dictionary<AddonVersion, IAddon> campaigns = new(2);

        var bloodId = nameof(GameEnum.Blood).ToLower();
        campaigns.Add(new(bloodId), new BloodCampaign()
        {
            Id = bloodId,
            Type = AddonTypeEnum.Official,
            Title = "Blood",
            GridImage = ImageHelper.FileNameToStream("Blood.blood.png", Assembly.GetExecutingAssembly()),
            Author = "Monolith Productions",
            Description = """
                **Blood** is a PC game released for MS-DOS on May 31, 1997. It was developed by **Monolith Productions** and published by **GT Interactive**.

                The game became well-known for its copious amounts of violence and numerous stylistic and cultural references to literary and cinematic horror works.
                It was also the first **Build engine** game to feature voxels and simulated room-over-room, which were both also seen in **Shadow Warrior** a few months later.

                The game's hero (or anti-hero) is a man named Caleb (voiced by Stephan Weyte), a merciless gunfighter born in Texas who serves a cult called "The Cabal" that worships the dark god Tchernobog
                (voiced by Monolith CEO Jason Hall, who was credited simply as "The Voice"). Caleb joined the cult after meeting Ophelia Price, a woman whose homestead was burned down by the Cabal,
                killing her husband and baby son. She blamed her spouse for their deaths, because he wanted to rescind his membership.
                Half-crazy and rambling, Caleb nursed her back to health. It is implied that she later became Caleb's lover, and introduced him to the cult.
                Together they rose to the highest ranks and became "The Chosen", the four most esteemed generals of Tchernobog's army (the other two being Ishmael and Gabriel).
                """,
            Version = null,
            SupportedGame = new(GameEnum.Blood),
            RequiredFeatures = null,
            PathToFile = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            INI = ClientConsts.BloodIni,
            RFF = null,
            SND = null,
            StartMap = null,
            PreviewImage = null,
            IsFolder = false,
            Executables = null
        });

        if (bGame.IsCrypticPassageInstalled)
        {
            var bloodCpId = nameof(BloodAddonEnum.BloodCP).ToLower();

            campaigns.Add(new(bloodCpId), new BloodCampaign()
            {
                Id = bloodCpId,
                Type = AddonTypeEnum.Official,
                Title = "Cryptic Passage",
                GridImage = ImageHelper.FileNameToStream("Blood.cp.jpg", Assembly.GetExecutingAssembly()),
                Author = "Sunstorm Interactive",
                Description = """
                    **Cryptic Passage** (originally titled Passage to Transylvania) is the first of two expansion packs for Blood.
                    It contain a new episode with ten single-player maps, and four new BloodBath maps.

                    Caleb travels to the Carpathian mountains after he hears of an ancient scroll taken from him.
                    This scroll is said to be capable of upsetting the balance in the otherworld, but Caleb finds himself detoured by the forces of Tchernobog and the Cabal.
                    He must find the scroll and take out everyone responsible for interrupting his journey.
                    """,
                Version = null,
                SupportedGame = new(GameEnum.Blood),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { nameof(BloodAddonEnum.BloodCP), null } },
                IncompatibleAddons = null,
                MainDef = null,
                AdditionalDefs = null,
                INI = ClientConsts.CrypticIni,
                RFF = null,
                SND = null,
                StartMap = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            });
        }

        return campaigns;
    }


    /// <summary>
    /// Get list of original campaigns
    /// </summary>
    private Dictionary<AddonVersion, IAddon> GetWangCampaigns()
    {
        Guard2.ThrowIfNotType<WangGame>(_game, out var wGame);

        Dictionary<AddonVersion, IAddon> campaigns = new(1);

        if (wGame.IsBaseGameInstalled)
        {
            var wangId = nameof(GameEnum.ShadowWarrior).ToLower();
            campaigns.Add(new(wangId), new WangCampaign()
            {
                Id = wangId,
                Type = AddonTypeEnum.Official,
                Title = "Shadow Warrior",
                GridImage = ImageHelper.FileNameToStream("Wang.wang.jpg", Assembly.GetExecutingAssembly()),
                Version = null,
                Author = "3D Realms",
                Description = """
                    **Shadow Warrior** is a first-person shooter developed by **3D Realms** and released on May 13, 1997 by **GT Interactive**.
                    
                    The premise of Shadow Warrior is that the protagonist, Chinese-Japanese, Lo Wang, worked as a bodyguard for Zilla Enterprises, a conglomerate that had power over every major industry in Japan.
                    However, this led to corruption, and Master Zilla - the president - planned to conquer Japan using creatures from the "dark side".
                    In discovery of this, Lo Wang quit his job as a bodyguard. Master Zilla realized that not having a warrior as powerful as Lo Wang would be dangerous, and sent his creatures to battle Lo Wang.
                    """,
                PathToFile = null,
                SupportedGame = new(GameEnum.ShadowWarrior),
                RequiredFeatures = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainDef = null,
                AdditionalDefs = null,
                StartMap = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            });
        }

        return campaigns;
    }


    /// <inheritdoc/>
    private Dictionary<AddonVersion, IAddon> GetFuryCampaigns()
    {
        Guard2.ThrowIfNotType<FuryGame>(_game, out var fGame);

        Dictionary<AddonVersion, IAddon> campaigns = new(1);

        if (fGame.IsBaseGameInstalled)
        {
            var furyId = nameof(GameEnum.Fury).ToLower();
            campaigns.Add(new(furyId), new DukeCampaign()
            {
                Id = furyId,
                Type = AddonTypeEnum.Official,
                Title = IsAftershock(fGame) ? "Ion Fury: Aftershock" : "Ion Fury",
                GridImage = IsAftershock(fGame) ? ImageHelper.FileNameToStream("Fury.aftershock.jpg", Assembly.GetExecutingAssembly()) : ImageHelper.FileNameToStream("Fury.fury.jpg", Assembly.GetExecutingAssembly()),
                Author = "Voidpoint, LLC",
                Description = """
                **Ion Fury** (originally titled Ion Maiden) is a 2019 cyberpunk first-person shooter developed by **Voidpoint** and published by **3D Realms**.

                It is a prequel to the 2016 video game Bombshell. Ion Fury runs on a modified version of Ken Silverman's Build engine and is the first original commercial game to utilize the engine in 20 years, the previous being World War II GI.

                You assume the role of Shelly "Bombshell" Harrison, a bomb disposal expert aligned to the Global Defense Force. Dr. Jadus Heskel, a transhumanist cult leader, unleashes an army of cybernetically-enhanced soldiers on the futuristic dystopian city of Neo D.C., which Shelly is tasked with fighting through.
                """,
                Version = null,
                SupportedGame = new(GameEnum.Fury),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
                MainDef = null,
                AdditionalDefs = null,
                RTS = null,
                StartMap = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            });
        }

        return campaigns;
    }

    /// <summary>
    /// Is Aftershock addon installed
    /// </summary>
    private bool IsAftershock(FuryGame fGame)
    {
        if (fGame.GameInstallFolder is null)
        {
            return false;
        }

        try
        {
            var text = File.ReadAllText(Path.Combine(fGame.GameInstallFolder, "fury.grpinfo"));

            if (text.Contains("ashock.def"))
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }


    /// <inheritdoc/>
    private Dictionary<AddonVersion, IAddon> GetRedneckCampaigns()
    {
        Guard2.ThrowIfNotType<RedneckGame>(_game, out var rGame);

        Dictionary<AddonVersion, IAddon> campaigns = new(3);

        if (rGame.IsBaseGameInstalled)
        {
            var redneckId = nameof(GameEnum.Redneck).ToLower();
            campaigns.Add(new(redneckId), new DukeCampaign()
            {
                Id = redneckId,
                Type = AddonTypeEnum.Official,
                Title = "Redneck Rampage",
                GridImage = ImageHelper.FileNameToStream("Redneck.redneck.jpg", Assembly.GetExecutingAssembly()),
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
                SupportedGame = new(GameEnum.Redneck),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
                MainDef = null,
                AdditionalDefs = null,
                RTS = null,
                StartMap = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            });

            if (rGame.IsRoute66Installed)
            {
                var redneckR66Id = nameof(RedneckAddonEnum.Route66).ToLower();
                campaigns.Add(new(redneckR66Id), new DukeCampaign()
                {
                    Id = redneckR66Id,
                    Type = AddonTypeEnum.Official,
                    Title = "Route 66",
                    GridImage = ImageHelper.FileNameToStream("Redneck.route66.jpg", Assembly.GetExecutingAssembly()),
                    Author = "Sunstorm Interactive",
                    Description = """
                        **Redneck Rampage: Suckin' Grits on Route 66** is a 12-level expansion pack for Redneck Rampage. It was developed by Sunstorm Interactive and released on December 19, 1997.
                        The add-on contains several new locations and textures, as well as a new ending.
                        """,
                    Version = null,
                    SupportedGame = new(GameEnum.Redneck),
                    RequiredFeatures = null,
                    PathToFile = null,
                    DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { nameof(RedneckAddonEnum.Route66), null } },
                    IncompatibleAddons = null,
                    MainCon = null,
                    AdditionalCons = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    RTS = null,
                    StartMap = null,
                    PreviewImage = null,
                    IsFolder = false,
                    Executables = null
                });
            }
        }

        if (rGame.IsAgainInstalled)
        {
            var redneckRaId = nameof(GameEnum.RidesAgain).ToLower();
            campaigns.Add(new(redneckRaId), new DukeCampaign()
            {
                Id = redneckRaId,
                Type = AddonTypeEnum.Official,
                Title = "Rides Again",
                GridImage = ImageHelper.FileNameToStream("Redneck.again.jpg", Assembly.GetExecutingAssembly()),
                Author = "Xatrix Entertainment",
                Description = """
                    **Redneck Rampage Rides Again** is a sequel to Redneck Rampage developed by **Xatrix Entertainment** and published by **Interplay Entertainment** for MS-DOS in 1998.

                    After Leonard and Bubba crash-land a UFO, they find themselves in the middle of the desert.
                    Along the way, they are hunted by aliens and must blast their way through jackalope farms, Disgraceland, a riverboat, a brothel and various other locales.
                    """,
                Version = null,
                SupportedGame = new(GameEnum.RidesAgain),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
                MainDef = null,
                AdditionalDefs = null,
                RTS = null,
                StartMap = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            });
        }

        return campaigns;
    }


    /// <inheritdoc/>
    private Dictionary<AddonVersion, IAddon> GetNamCampaigns()
    {
        Guard2.ThrowIfNotType<NamGame>(_game, out var nGame);

        Dictionary<AddonVersion, IAddon> campaigns = new(1);

        if (nGame.IsBaseGameInstalled)
        {
            var namId = nameof(GameEnum.NAM).ToLower();
            campaigns.Add(new(namId), new DukeCampaign()
            {
                Id = namId,
                Type = AddonTypeEnum.Official,
                Title = "NAM",
                GridImage = ImageHelper.FileNameToStream("NAM.nam.jpg", Assembly.GetExecutingAssembly()),
                Author = "TNT Team",
                Description = """
                    You are Alan 'The Bear' Westmoreland, Marine Corps sergeant. The trouble starts on a deadly Viet Cong raid. Here the jungle is your battleground.

                    Your mission, survive.

                    NAM captures all of intensity and paranoia of jungle warfare. Fire-fights, ambushes, booby-traps, snipers, air-strikes, anti-personnel mines AND MORE.

                    Feel the tropical heat and the fear of tunnel skirmishes, paddy killing fields, swamps and thick jungles.

                    NAM is the first game of its kind. NAM IS WAR!
                    """,
                Version = null,
                SupportedGame = new(GameEnum.NAM),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
                RTS = null,
                MainDef = null,
                AdditionalDefs = null,
                StartMap = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            });
        }

        return campaigns;
    }


    /// <inheritdoc/>
    private Dictionary<AddonVersion, IAddon> GetWw2Campaigns()
    {
        Guard2.ThrowIfNotType<WW2GIGame>(_game, out var wGame);

        Dictionary<AddonVersion, IAddon> campaigns = new(1);

        if (wGame.IsBaseGameInstalled)
        {
            var ww2id = nameof(GameEnum.WW2GI).ToLower();
            campaigns.Add(new(ww2id), new DukeCampaign()
            {
                Id = ww2id,
                Type = AddonTypeEnum.Official,
                Title = "World War II GI",
                GridImage = ImageHelper.FileNameToStream("WW2GI.ww2gi.jpg", Assembly.GetExecutingAssembly()),
                Author = "TNT Team",
                Description = """
                    **WWII GI** is the invasion of Normandy. The paranoia. The fear. the intensity that was D-Day. You will experience it first hand.

                    You're in the 101st Airborne, part of the first wave of allied forces to touch down in a no-man's land of twisted shrapnel, dead bodies and heavily armed Nazi-infested machine-gun bunkers. Now you must fight your way through hostile beaches, abandoned country roads with tall, sniper infested hedgerows, the narrow streets of devastated villages and more.

                    This is D-Day!
                    """,
                Version = null,
                SupportedGame = new(GameEnum.WW2GI),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
                RTS = null,
                MainDef = null,
                AdditionalDefs = null,
                StartMap = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            });
        }

        if (wGame.IsPlatoonInstalled)
        {
            var platoon = nameof(WW2GIAddonEnum.Platoon).ToLower();
            campaigns.Add(new(platoon), new DukeCampaign()
            {
                Id = platoon,
                Type = AddonTypeEnum.Official,
                Title = "Platoon Leader",
                GridImage = ImageHelper.FileNameToStream("WW2GI.platoon.jpg", Assembly.GetExecutingAssembly()),
                Author = "TNT Team",
                Description = """
                    **Platoon Leader** is an add-on for GT Interactive game WWII GI.

                    This add-on features three single-player-only levels: one WWII Pacific and two Vietnam War scenarios. Includes many new effects and features not seen in the game WWII GI.
                    """,
                Version = null,
                SupportedGame = new(GameEnum.WW2GI),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
                RTS = null,
                MainDef = null,
                AdditionalDefs = null,
                StartMap = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            });
        }

        return campaigns;
    }


    /// <inheritdoc/>
    private Dictionary<AddonVersion, IAddon> GetTekWarCampaigns()
    {
        Guard2.ThrowIfNotType<TekWarGame>(_game, out var tGame);

        Dictionary<AddonVersion, IAddon> campaigns = new(1);

        if (tGame.IsBaseGameInstalled)
        {
            var namId = nameof(GameEnum.TekWar).ToLower();
            campaigns.Add(new(namId), new DukeCampaign()
            {
                Id = namId,
                Type = AddonTypeEnum.Official,
                Title = "TekWar",
                GridImage = ImageHelper.FileNameToStream("TekWar.tekwar.jpg", Assembly.GetExecutingAssembly()),
                Author = "Capstone Software",
                Description = """
                    You're an ex-cop who was sentenced to cryo sleep. When you awake you are recruited by the Cosmos Detective Agency as a hitman. Why? Cause there's a dangerous new drug on the streets of New LA: Tek!
                    
                    Take out the seven Tek Lords and their minions in 7 missions, but spare those innocent civilians.
                    """,
                Version = null,
                SupportedGame = new(GameEnum.TekWar),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
                RTS = null,
                MainDef = null,
                AdditionalDefs = null,
                StartMap = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            });
        }

        return campaigns;
    }


    /// <inheritdoc/>
    private Dictionary<AddonVersion, IAddon> GetSlaveCampaigns()
    {
        Guard2.ThrowIfNotType<SlaveGame>(_game, out var sGame);

        Dictionary<AddonVersion, IAddon> campaigns = new(1);

        if (sGame.IsBaseGameInstalled)
        {
            var slaveId = nameof(GameEnum.Exhumed).ToLower();
            campaigns.Add(new(slaveId), new SlaveCampaign()
            {
                Id = slaveId,
                Type = AddonTypeEnum.Official,
                Title = "Powerslave",
                GridImage = ImageHelper.FileNameToStream("Slave.slave.jpg", Assembly.GetExecutingAssembly()),
                Author = "Lobotomy Software",
                Description = """
                    **PowerSlave**, known as **Exhumed** in Europe and **1999 AD: Resurrection of the Pharaoh** in Japan, is a first-person shooter video game developed by **Lobotomy Software**
                    and published by **Playmates Interactive Entertainment** in North America, and **BMG Interactive** in Europe and Japan.
                    It was released in North America, Europe and Japan, for the Sega Saturn, PlayStation, and MS-DOS over the course of a year from late 1996 to late 1997.

                    PowerSlave is set in and around the ancient Egyptian city of Karnak in the late 20th century. The city has been seized by unknown forces, with a special crack team of
                    hardened soldiers sent to the valley of Karnak, to uncover the source of this trouble. However, on the journey there, the player's helicopter is shot down and the player barely escapes.
                    The player is sent in to the valley as the hero to save Karnak and the World. The player must battle hordes of extraterrestrial insectoid beings known as the Kilmaat, as well as their
                    various minions, which include mummies, Anubis soldiers, scorpions, and evil spirits. The player's course of action is directed by the spirit of King Ramses, whose mummy was exhumed
                    from its tomb by the Kilmaat, who seek to resurrect him and use his powers to control the world.
                    """,
                Version = null,
                SupportedGame = new(GameEnum.Exhumed),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainDef = null,
                AdditionalDefs = null,
                StartMap = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            });
        }

        return campaigns;
    }


    /// <inheritdoc/>
    private Dictionary<AddonVersion, IAddon> GetWitchavenCampaigns()
    {
        Guard2.ThrowIfNotType<WitchavenGame>(_game, out var wGame);

        Dictionary<AddonVersion, IAddon> campaigns = new(1);

        if (wGame.IsBaseGameInstalled)
        {
            var namId = nameof(GameEnum.Witchaven).ToLower();
            campaigns.Add(new(namId), new DukeCampaign()
            {
                Id = namId,
                Type = AddonTypeEnum.Official,
                Title = "Witchaven",
                GridImage = ImageHelper.FileNameToStream("Witchaven.wh1.jpg", Assembly.GetExecutingAssembly()),
                Author = "Capstone Software",
                Description = """
                    Descend into a dark and gruesome nightmare!
                    
                    You alone must face the evil within the volcanic pit of the Island of Char, toward the mystical lair of Witchaven. Confront witches that have cast a shadow of evil spells shrouding you in the never-ending darkness. Make use of your magic, might, and mind as you engage in bloody warfare with vile demons and monsters. Use medieval weapons to destroy these creatures of the night and cease the chaos.
                    
                    Dare to enter this 3D Hell... Dare to enter Witchaven!
                    """,
                Version = null,
                SupportedGame = new(GameEnum.Witchaven),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
                RTS = null,
                MainDef = null,
                AdditionalDefs = null,
                StartMap = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            });
        }

        if (wGame.IsWitchaven2Installed)
        {
            var namId = nameof(GameEnum.Witchaven2).ToLower();
            campaigns.Add(new(namId), new DukeCampaign()
            {
                Id = namId,
                Type = AddonTypeEnum.Official,
                Title = "Witchaven II",
                GridImage = ImageHelper.FileNameToStream("Witchaven.wh2.jpg", Assembly.GetExecutingAssembly()),
                Author = "Capstone Software",
                Description = """
                    The witches have been destroyed in their lair on the Island of Char!
                    
                    Returning to your homeland, you are greeted with newborn hope, pride, and great celebration. After the revelry, you awaken to a dawn filled with an eerie silence that looms in the still air. Your countrymen are gone!

                    The great witch, Circa-Argoth has taken them to avenge the death of her sister. You have only yourself and your foolish meddling to blame. But, you are not meant to die... yet!
                    
                    Alone in the land that you have fought so fiercely to protect, you must gather your strength and use your anger to fight for Blood Vengeance.
                    """,
                Version = null,
                SupportedGame = new(GameEnum.Witchaven2),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
                RTS = null,
                MainDef = null,
                AdditionalDefs = null,
                StartMap = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            });
        }

        return campaigns;
    }
}
