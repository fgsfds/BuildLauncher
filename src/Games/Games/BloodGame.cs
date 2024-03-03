using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Mods.Mods;
using Mods.Providers;

namespace Games.Games
{
    public sealed class BloodGame(InstalledModsProvider modsProvider) : BaseGame(modsProvider)
    {
        /// <inheritdoc/>
        public override GameEnum GameEnum => GameEnum.Blood;

        /// <inheritdoc/>
        public override string FullName => "Blood";

        /// <inheritdoc/>
        public override string ShortName => FullName;

        /// <inheritdoc/>
        public override string MainFile => "BLOOD.RFF";

        /// <inheritdoc/>
        public override string StartupFile => "blood.def";

        /// <inheritdoc/>
        public override List<string> RequiredFiles => [MainFile, "BLOOD.INI", "GUI.RFF", "SOUNDS.RFF", "SURFACE.DAT", "TILES000.ART", "VOXEL.DAT"];

        /// <summary>
        /// List of files required for Cryptic Passage
        /// </summary>
        private readonly List<string> RequiredCPFiles = ["CP01.MAP", "CPART07.AR_", "CPART15.AR_", "CRYPTIC.INI", "CRYPTIC.SMK", "CRYPTIC.WAV"];

        /// <summary>
        /// Is Cryptic Passage instaleld
        /// </summary>
        public bool IsCrypticPassageInstalled => IsInstalled(RequiredCPFiles);


        /// <inheritdoc/>
        protected override List<IMod> GetOriginalCampaigns()
        {
            List<IMod> campaigns = new();

            campaigns.Add(new BloodCampaign()
            {
                ModType = ModTypeEnum.Campaign,
                DisplayName = "Blood",
                StartupFile = Consts.BloodIni,
                Image = ImageHelper.FileNameToStream("Blood.blood.png"),
                AddonEnum = BloodAddonEnum.Blood,
                Version = null,
                SupportedPorts = null,
                Description = null,
                Url = null,
                Author = null,
                IsOfficial = true,
                PathToFile = null
            });

            if (IsCrypticPassageInstalled)
            {
                campaigns.Add(new BloodCampaign()
                {
                    ModType = ModTypeEnum.Campaign,
                    DisplayName = "Cryptic Passage",
                    StartupFile = Consts.CrypticIni,
                    Image = ImageHelper.FileNameToStream("Blood.cp.png"),
                    AddonEnum = BloodAddonEnum.Cryptic,
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
