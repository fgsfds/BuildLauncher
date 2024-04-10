using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Mods.Mods;
using Mods.Providers;

namespace Games.Games
{
    public sealed class BloodGame(
        InstalledModsProviderFactory modsProvider,
        DownloadableModsProviderFactory downloadableModsProviderFactory
        ) : BaseGame(modsProvider, downloadableModsProviderFactory)
    {
        /// <inheritdoc/>
        public override GameEnum GameEnum => GameEnum.Blood;

        /// <inheritdoc/>
        public override string FullName => "Blood";

        /// <inheritdoc/>
        public override string ShortName => FullName;

        /// <inheritdoc/>
        public override string DefFileName => "blood.def";

        /// <inheritdoc/>
        public override List<string> RequiredFiles => [Consts.BloodIni, "BLOOD.RFF", "GUI.RFF", "SOUNDS.RFF", "SURFACE.DAT", "TILES000.ART", "VOXEL.DAT"];

        /// <summary>
        /// List of files required for Cryptic Passage
        /// </summary>
        private readonly List<string> RequiredCPFiles = [Consts.CrypticIni, "CP01.MAP", "CPART07.AR_", "CPART15.AR_", "CRYPTIC.SMK", "CRYPTIC.WAV"];

        /// <summary>
        /// Is Cryptic Passage instaleld
        /// </summary>
        public bool IsCrypticPassageInstalled => IsInstalled(RequiredCPFiles);


        /// <inheritdoc/>
        protected override Dictionary<string, IAddon> GetOriginalCampaigns()
        {
            Dictionary<string, IAddon> campaigns = new(2);

            campaigns.Add(GameEnum.Blood.ToString(), new BloodCampaign()
            {
                Id = GameEnum.Blood.ToString(),
                Type = ModTypeEnum.Official,
                Title = "Blood",
                Image = ImageHelper.FileNameToStream("Blood.blood.png"),
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
                SupportedPorts = null,
                PathToFile = null,
                SupportedGames = null,
                SupportedGamesCrcs = null,
                Dependencies = null,
                Incompatibles = null,
                MainDef = null,
                AdditionalDefs = null,
                INI = Consts.BloodIni,
                RFF = null,
                SND = null,
                StartMap = null,
                RequiredAddonEnum = BloodAddonEnum.Blood
            });

            if (IsCrypticPassageInstalled)
            {
                campaigns.Add(BloodAddonEnum.BloodCP.ToString(), new BloodCampaign()
                {
                    Id = BloodAddonEnum.BloodCP.ToString(),
                    Type = ModTypeEnum.Official,
                    Title = "Cryptic Passage",
                    Image = ImageHelper.FileNameToStream("Blood.cp.jpg"),
                    Author = "Sunstorm Interactive",
                    Description = """
                        **Cryptic Passage** (originally titled Passage to Transylvania) is the first of two expansion packs for Blood.
                        It contain a new episode with ten single-player maps, and four new BloodBath maps.

                        Caleb travels to the Carpathian mountains after he hears of an ancient scroll taken from him.
                        This scroll is said to be capable of upsetting the balance in the otherworld, but Caleb finds himself detoured by the forces of Tchernobog and the Cabal.
                        He must find the scroll and take out everyone responsible for interrupting his journey.
                        """,
                    Version = null,
                    SupportedPorts = null,
                    PathToFile = null,
                    SupportedGames = null,
                    SupportedGamesCrcs = null,
                    Dependencies = null,
                    Incompatibles = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    INI = Consts.CrypticIni,
                    RFF = null,
                    SND = null,
                    StartMap = null,
                    RequiredAddonEnum = BloodAddonEnum.BloodCP
                });
            }

            return campaigns;
        }
    }
}
