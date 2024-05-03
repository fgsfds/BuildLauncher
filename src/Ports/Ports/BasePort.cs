using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Common.Releases;
using Games.Games;
using Mods.Addons;
using Mods.Serializable.Addon;
using System.Text;

namespace Ports.Ports
{
    /// <summary>
    /// Base class for ports
    /// </summary>
    public abstract class BasePort
    {
        /// <summary>
        /// Port enum
        /// </summary>
        public abstract PortEnum PortEnum { get; }

        /// <summary>
        /// Main executable
        /// </summary>
        public abstract string Exe { get; }

        /// <summary>
        /// Name of the port
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Games supported by the port
        /// </summary>
        public abstract List<GameEnum> SupportedGames { get; }

        /// <summary>
        /// Url to the port repository
        /// </summary>
        public abstract Uri RepoUrl { get; }

        /// <summary>
        /// Predicate for Windows release
        /// </summary>
        public abstract Func<GitHubReleaseAsset, bool> WindowsReleasePredicate { get; }

        /// <summary>
        /// Currently installed version
        /// </summary>
        public abstract string? InstalledVersion { get; }

        /// <summary>
        /// Path to port install folder
        /// </summary>
        public virtual string PathToExecutableFolder => Path.Combine(CommonProperties.PortsFolderPath, PortFolderName);

        /// <summary>
        /// Is port installed
        /// </summary>
        public virtual bool IsInstalled => InstalledVersion is not null;

        /// <summary>
        /// Path to port exe
        /// </summary>
        public string FullPathToExe => Path.Combine(PathToExecutableFolder, Exe);

        /// <summary>
        /// Name of the config file
        /// </summary>
        protected abstract string ConfigFile { get; }

        /// <summary>
        /// Cmd parameter to add folder to search path
        /// </summary>
        protected abstract string AddDirectoryParam { get; }

        /// <summary>
        /// Cmd parameter to load additional GRP file
        /// </summary>
        protected abstract string AddGrpParam { get; }

        /// <summary>
        /// Cmd parameter to load additional file
        /// </summary>
        protected abstract string AddFileParam { get; }

        /// <summary>
        /// Cmd parameter to load additional Def file
        /// </summary>
        protected abstract string AddDefParam { get; }

        /// <summary>
        /// Cmd parameter to load additional Con file
        /// </summary>
        protected abstract string AddConParam { get; }

        /// <summary>
        /// Cmd parameter to load main Def file
        /// </summary>
        protected abstract string MainDefParam { get; }

        /// <summary>
        /// Cmd parameter to load main Con file
        /// </summary>
        protected abstract string MainConParam { get; }

        /// <summary>
        /// Name of the folder that contains the port files
        /// By default is the same as <see cref="Name"/>
        /// </summary>
        protected virtual string PortFolderName => Name;

        /// <summary>
        /// Port's icon
        /// </summary>
        public Stream Icon => ImageHelper.FileNameToStream($"{Name}.png");


        /// <summary>
        /// Get command line parameters to start the game with selected campaign and autoload mods
        /// </summary>
        /// <param name="game">Game<param>
        /// <param name="addon">Map/campaign</param>
        /// <param name="skipIntro">Skip intro</param>
        public string GetStartGameArgs(IGame game, IAddon addon, bool skipIntro, bool skipStartup)
        {
            StringBuilder sb = new();

            BeforeStart(game, addon);

            if (skipIntro)
            {
                GetSkipIntroParameter(sb);
            }

            if (skipStartup)
            {
                GetSkipStartupParameter(sb);
            }

            GetStartCampaignArgs(sb, game, addon);

            GetAutoloadModsArgs(sb, game, addon);

            return sb.ToString();
        }


        /// <summary>
        /// Get startup args for packed and loose maps
        /// </summary>
        protected void GetMapArgs(StringBuilder sb, IGame game, IAddon camp)
        {
            //TODO loose maps
            //TODO e#m#
            if (camp.StartMap is MapFileDto mapFile)
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(game.MapsFolderPath, camp.FileName!)}""");
                sb.Append($@" -map ""{mapFile.File}""");
            }
            else
            {
                ThrowHelper.NotImplementedException();
            }
        }


        /// <summary>
        /// Check if autoload mod works with current port and addon
        /// </summary>
        /// <param name="autoloadMod">Autoload mod</param>
        /// <param name="campaign">Campaign</param>
        protected bool ValidateAutoloadMod(AutoloadMod autoloadMod, IAddon campaign, Dictionary<string, IAddon> addons)
        {
            if (!autoloadMod.IsEnabled)
            {
                //skipping disabled mods
                return false;
            }

            if (!autoloadMod.SupportedPorts?.Contains(PortEnum) ?? false)
            {
                //skipping mods not supported by the current port
                return false;
            }

            if (autoloadMod.Dependencies is not null)
            {
                foreach (var dep in autoloadMod.Dependencies)
                {
                    if (!addons.ContainsKey(dep.Key) &&
                        !campaign.Id.Equals(dep.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        //skipping mods that don't have every dependency
                        return false;
                    }
                }
            }

            if (autoloadMod.Incompatibles is not null)
            {
                foreach (var dep in autoloadMod.Incompatibles)
                {
                    if (addons.ContainsKey(dep.Key) ||
                        campaign.Id.Equals(dep.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        //skipping incompatible mods
                        return false;
                    }
                }
            }

            //hack for RR and RA
            if (autoloadMod.SupportedGames is not null &&
                campaign is RedneckCampaign rCamp)
            {
                var game = rCamp.RequiredAddonEnum switch
                {
                    RedneckAddonEnum.Redneck or RedneckAddonEnum.RedneckR66 => GameEnum.Redneck,
                    _ => GameEnum.RedneckRA,
                };

                if (!autoloadMod.SupportedGames.Contains(game))
                {
                    return false;
                }
            }

            return true;
        }


        protected void GetBloodArgs(StringBuilder sb, BloodGame game, BloodCampaign bCamp)
        {
            if (bCamp.INI is not null)
            {
                sb.Append($@" -ini ""{bCamp.INI}""");
            }
            else if (bCamp.RequiredAddonEnum is BloodAddonEnum.BloodCP)
            {
                sb.Append($@" -ini ""{Consts.CrypticIni}""");
            }


            if (bCamp.FileName is null)
            {
                return;
            }


            if (bCamp.RFF is not null)
            {
                sb.Append($@" -rff {bCamp.RFF}");
            }


            if (bCamp.SND is not null)
            {
                sb.Append($@" -snd {bCamp.SND}");
            }


            if (bCamp.Type is AddonTypeEnum.TC)
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, bCamp.FileName)}""");
            }
            else if (bCamp.Type is AddonTypeEnum.Map)
            {
                GetMapArgs(sb, game, bCamp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {bCamp.Type} is not supported");
                return;
            }
        }


        protected void GetSlaveArgs(StringBuilder sb, SlaveGame sGame, SlaveCampaign sCamp)
        {
            if (sCamp.FileName is null)
            {
                return;
            }


            if (sCamp.Type is AddonTypeEnum.TC)
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(sGame.CampaignsFolderPath, sCamp.FileName)}""");
            }
            else if (sCamp.Type is AddonTypeEnum.Map)
            {
                GetMapArgs(sb, sGame, sCamp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {sCamp.Type} is not supported");
                return;
            }
        }


        /// <summary>
        /// Method to perform before starting the port
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="campaign">Campaign</param>
        protected virtual void BeforeStart(IGame game, IAddon campaign) { }

        /// <summary>
        /// Get command line arguments to start custom map or campaign
        /// </summary>
        /// <param name="sb">String builder for parameters</param>
        /// <param name="game">Game<param>
        /// <param name="addon">Map/campaign</param>
        protected abstract void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon);

        /// <summary>
        /// Get command line arguments to load mods
        /// </summary>
        /// <param name="sb">String builder for parameters</param>
        /// <param name="game">Game<param>
        /// <param name="campaign">Campaign\map<param>
        protected abstract void GetAutoloadModsArgs(StringBuilder sb, IGame game, IAddon campaign);

        /// <summary>
        /// Return command line parameter to skip intro
        /// </summary>
        /// <param name="sb">String builder for parameters</param>
        protected abstract void GetSkipIntroParameter(StringBuilder sb);

        /// <summary>
        /// Return command line parameter to skip startup window
        /// </summary>
        /// <param name="sb">String builder for parameters</param>
        protected abstract void GetSkipStartupParameter(StringBuilder sb);
    }
}
