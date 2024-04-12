using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Mods.Mods;
using Mods.Providers;

namespace Games.Games
{
    public sealed class SlaveGame(InstalledModsProviderFactory modsProvider, DownloadableModsProviderFactory downloadableModsProviderFactory) : BaseGame(modsProvider, downloadableModsProviderFactory)
    {
        /// <inheritdoc/>
        public override GameEnum GameEnum => GameEnum.Exhumed;

        /// <inheritdoc/>
        public override string FullName => "Powerslave";

        /// <inheritdoc/>
        public override string ShortName => "Slave";

        /// <inheritdoc/>
        public override List<string> RequiredFiles => ["STUFF.DAT"];


        /// <inheritdoc/>
        protected override Dictionary<string, IAddon> GetOriginalCampaigns()
        {
            Dictionary<string, IAddon> campaigns = new(1, StringComparer.OrdinalIgnoreCase);

            if (IsBaseGameInstalled)
            {
                campaigns.Add(GameEnum.Exhumed.ToString(), new SlaveCampaign()
                {
                    Id = GameEnum.Exhumed.ToString(),
                    Type = AddonTypeEnum.Official,
                    Title = "Powerslave",
                    Image = ImageHelper.FileNameToStream("Slave.slave.jpg"),
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
                    SupportedPorts = null,
                    PathToFile = null,
                    SupportedGames = null,
                    SupportedGamesCrcs = null,
                    Dependencies = null,
                    Incompatibles = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    StartMap = null
                });
            }

            return campaigns;
        }
    }
}
