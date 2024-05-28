﻿using Common.Enums;
using Common.Enums.Addons;
using Common.Enums.Versions;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
using System.Diagnostics;
using System.Text;

namespace Ports.Ports
{
    /// <summary>
    /// Raze port
    /// </summary>
    public sealed class Raze : BasePort
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.Raze;

        /// <inheritdoc/>
        public override string Exe => "raze.exe";

        /// <inheritdoc/>
        public override string Name => "Raze";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames =>
            [
            GameEnum.Blood,
            GameEnum.Duke3D,
            GameEnum.ShadowWarrior,
            GameEnum.Exhumed,
            GameEnum.Redneck,
            GameEnum.RidesAgain,
            GameEnum.NAM,
            GameEnum.WWIIGI
            ];

        /// <inheritdoc/>
        public override string? InstalledVersion =>
            File.Exists(FullPathToExe)
            ? FileVersionInfo.GetVersionInfo(FullPathToExe).FileVersion
            : null;


        /// <inheritdoc/>
        protected override string ConfigFile => "raze_portable.ini";

        /// <inheritdoc/>
        protected override string AddDirectoryParam => "-file ";

        /// <inheritdoc/>
        protected override string AddFileParam => "-file ";

        /// <inheritdoc/>
        protected override string AddDefParam => "-adddef ";

        /// <inheritdoc/>
        protected override string AddConParam => "-addcon ";

        /// <inheritdoc/>
        protected override string MainDefParam => "-def ";

        /// <inheritdoc/>
        protected override string MainConParam => "-con ";

        /// <inheritdoc/>
        protected override string AddGrpParam => throw new NotImplementedException();

        /// <inheritdoc/>
        public override List<FeatureEnum> SupportedFeatures => [FeatureEnum.WorldTourSupport, FeatureEnum.VacaDcSupport];

        /// <inheritdoc/>
        protected override void GetSkipIntroParameter(StringBuilder sb) => sb.Append(" -quick");

        /// <inheritdoc/>
        protected override void GetSkipStartupParameter(StringBuilder sb) => sb.Append(" -nosetup");


        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IAddon campaign)
        {
            var config = Path.Combine(PathToExecutableFolder, ConfigFile);

            if (!File.Exists(config))
            {
                //creating default config if it doesn't exist
                const string? DefaultConfig = """
                    [GameSearch.Directories]
                    Path=.

                    [FileSearch.Directories]
                    Path=.

                    [SoundfontSearch.Directories]
                    Path=$PROGDIR/soundfonts

                    [GlobalSettings]
                    gl_texture_filter=6
                    snd_alresampler=Nearest
                    gl_tonemap=5
                    hw_useindexedcolortextures=true
                    mus_extendedlookup=true
                    snd_extendedlookup=true
                    """;

                if (!Directory.Exists(Path.GetDirectoryName(config)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(config)!);
                }

                File.WriteAllText(config, DefaultConfig);
            }

            AddGamePathsToConfig(game.GameInstallFolder, game.ModsFolderPath, config);

            FixRoute66Files(game, campaign);
        }

        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon)
        {
            sb.Append($@" -savedir ""{Path.Combine(PathToExecutableFolder, "Save", addon.Id.Replace(' ', '_'))}""");

            if (addon.MainDef is not null)
            {
                sb.Append($@" {MainDefParam}""{addon.MainDef}""");
            }
            else
            {
                //overriding default def so gamename.def files are ignored
                sb.Append($@" {MainDefParam}""a""");
            }

            if (addon.AdditionalDefs is not null)
            {
                foreach (var def in addon.AdditionalDefs)
                {
                    sb.Append($@" {AddDefParam}""{def}""");
                }
            }


            if (game is DukeGame dGame && addon is DukeCampaign dMod)
            {
                GetDukeArgs(sb, dGame, dMod);
            }
            else if (game is BloodGame bGame && addon is BloodCampaign bCamp)
            {
                GetBloodArgs(sb, bGame, bCamp);
            }
            else if (game is WangGame wGame && addon is WangCampaign wCamp)
            {
                GetWangArgs(sb, wGame, wCamp);
            }
            else if (game is SlaveGame sGame && addon is SlaveCampaign sCamp)
            {
                GetSlaveArgs(sb, sGame, sCamp);
            }
            else if (game is RedneckGame rGame && addon is RedneckCampaign rCamp)
            {
                GetRedneckArgs(sb, rGame, rCamp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {addon} for game {game} is not supported");
            }
        }

        private void GetDukeArgs(StringBuilder sb, DukeGame game, DukeCampaign camp)
        {
            if (camp.SupportedGame.GameVersion is not null &&
                camp.SupportedGame.GameVersion.Equals(DukeVersionEnum.Duke3D_WT.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                var config = Path.Combine(PathToExecutableFolder, ConfigFile);
                AddGamePathsToConfig(game.DukeWTInstallPath, game.ModsFolderPath, config);

                sb.Append($" -addon {(byte)DukeAddonEnum.Base}");
            }
            else
            {
                byte addon = (byte)DukeAddonEnum.Base;

                if (camp.DependentAddons is null)
                {
                    addon = (byte)DukeAddonEnum.Base;
                }
                else if (camp.DependentAddons.ContainsKey(DukeAddonEnum.DukeDC.ToString()))
                {
                    addon = (byte)DukeAddonEnum.DukeDC;
                }
                else if (camp.DependentAddons.ContainsKey(DukeAddonEnum.DukeNW.ToString()))
                {
                    addon = (byte)DukeAddonEnum.DukeNW;
                }
                else if (camp.DependentAddons.ContainsKey(DukeAddonEnum.DukeVaca.ToString()))
                {
                    addon = (byte)DukeAddonEnum.DukeVaca;
                }

                sb.Append($@" -addon {addon}");
            }


            if (camp.FileName is null)
            {
                return;
            }


            if (camp.Type is AddonTypeEnum.TC)
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");

                if (camp.MainCon is not null)
                {
                    sb.Append($@" {MainConParam}""{camp.MainCon}""");
                }

                if (camp.AdditionalCons?.Count > 0)
                {
                    foreach (var con in camp.AdditionalCons)
                    {
                        sb.Append($@" {AddConParam}""{con}""");
                    }
                }
            }
            else if (camp.Type is AddonTypeEnum.Map)
            {
                GetMapArgs(sb, game, camp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {camp.Type} is not supported");
                return;
            }
        }

        private void GetWangArgs(StringBuilder sb, WangGame game, WangCampaign camp)
        {
            if (camp.DependentAddons is not null &&
                camp.DependentAddons.ContainsKey(WangAddonEnum.Wanton.ToString()))
            {
                sb.Append($" {AddFileParam}WT.GRP");
            }
            else if (camp.DependentAddons is not null &&
                camp.DependentAddons.ContainsKey(WangAddonEnum.TwinDragon.ToString()))
            {
                sb.Append($" {AddFileParam}TD.GRP");
            }


            if (camp.FileName is null)
            {
                return;
            }


            if (camp.Type is AddonTypeEnum.TC)
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");
            }
            else if (camp.Type is AddonTypeEnum.Map)
            {
                GetMapArgs(sb, game, camp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {camp.Type} is not supported");
                return;
            }
        }

        private void GetRedneckArgs(StringBuilder sb, RedneckGame game, RedneckCampaign camp)
        {
            if (camp.DependentAddons is not null &&
                camp.DependentAddons.ContainsKey(RedneckAddonEnum.Route66.ToString()))
            {
                sb.Append(" -route66");
                return;
            }

            if (camp.Id.Equals(nameof(GameEnum.RidesAgain), StringComparison.OrdinalIgnoreCase))
            {
                var config = Path.Combine(PathToExecutableFolder, ConfigFile);
                AddGamePathsToConfig(game.AgainInstallPath, game.ModsFolderPath, config);
            }


            if (camp.FileName is null)
            {
                return;
            }


            if (camp.Type is AddonTypeEnum.TC)
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");

                if (camp.MainCon is not null)
                {
                    sb.Append($@" {MainConParam}""{camp.MainCon}""");
                }

                if (camp.AdditionalCons?.Count > 0)
                {
                    foreach (var con in camp.AdditionalCons)
                    {
                        sb.Append($@" {AddConParam}""{con}""");
                    }
                }
            }
            else if (camp.Type is AddonTypeEnum.Map)
            {
                GetMapArgs(sb, game, camp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {camp.Type} is not supported");
                return;
            }
        }

        /// <inheritdoc/>
        protected override void GetAutoloadModsArgs(StringBuilder sb, IGame game, IAddon campaign)
        {
            var addons = game.GetAutoloadMods(true);

            if (addons.Count == 0)
            {
                return;
            }

            foreach (var addon in addons)
            {
                if (addon.Value is not AutoloadMod aMod)
                {
                    continue;
                }

                if (!ValidateAutoloadMod(aMod, campaign, addons))
                {
                    continue;
                }

                sb.Append($@" {AddFileParam}""{aMod.FileName}""");

                if (aMod.AdditionalDefs is not null)
                {
                    foreach (var def in aMod.AdditionalDefs)
                    {
                        sb.Append($@" {AddDefParam}""{def}""");
                    }
                }

                if (aMod.AdditionalCons is not null)
                {
                    foreach (var con in aMod.AdditionalCons)
                    {
                        sb.Append($@" {AddConParam}""{con}""");
                    }
                }
            }
        }

        /// <summary>
        /// Remove route 66 art files overrides used for RedNukem
        /// </summary>
        private void FixRoute66Files(IGame game, IAddon _)
        {
            game.GameInstallFolder.ThrowIfNull();

            if (game is RedneckGame)
            {
                var tilesA2 = Path.Combine(game.GameInstallFolder, "TILES024.ART");
                var tilesB2 = Path.Combine(game.GameInstallFolder, "TILES025.ART");
                var turdMovAnm2 = Path.Combine(game.GameInstallFolder, "TURDMOV.ANM");
                var turdMovVoc2 = Path.Combine(game.GameInstallFolder, "TURDMOV.VOC");
                var endMovAnm2 = Path.Combine(game.GameInstallFolder, "RR_OUTRO.ANM");
                var endMovVoc2 = Path.Combine(game.GameInstallFolder, "LN_FINAL.VOC");

                if (File.Exists(tilesA2))
                {
                    File.Delete(tilesA2);
                }

                if (File.Exists(tilesB2))
                {
                    File.Delete(tilesB2);
                }

                if (File.Exists(turdMovAnm2))
                {
                    File.Delete(turdMovAnm2);
                }

                if (File.Exists(turdMovVoc2))
                {
                    File.Delete(turdMovVoc2);
                }

                if (File.Exists(endMovAnm2))
                {
                    File.Delete(endMovAnm2);
                }

                if (File.Exists(endMovVoc2))
                {
                    File.Delete(endMovVoc2);
                }
            }
        }

        /// <summary>
        /// Add paths to game and mods folder to the config
        /// </summary>
        [Obsolete("Remove if this ever implemented https://github.com/ZDoom/Raze/issues/1060")]
        private static void AddGamePathsToConfig(string gameFolder, string modsFolder, string config)
        {
            var contents = File.ReadAllLines(config);

            StringBuilder sb = new(contents.Length);

            for (var i = 0; i < contents.Length; i++)
            {
                if (contents[i].Equals("[GameSearch.Directories]"))
                {
                    sb.AppendLine(contents[i]);

                    var path = gameFolder.Replace('\\', '/');
                    sb.Append("Path=").AppendLine(path);

                    do
                    {
                        i++;
                    }
                    while (!string.IsNullOrWhiteSpace(contents[i]));

                    sb.AppendLine();
                    continue;
                }

                if (contents[i].Equals("[FileSearch.Directories]"))
                {
                    sb.AppendLine(contents[i]);

                    var path = gameFolder.Replace('\\', '/');
                    sb.Append("Path=").AppendLine(path);
                    path = modsFolder.Replace('\\', '/');
                    sb.Append("Path=").AppendLine(path);

                    do
                    {
                        i++;
                    }
                    while (!string.IsNullOrWhiteSpace(contents[i]));

                    sb.AppendLine();
                    continue;
                }

                sb.AppendLine(contents[i]);
            }

            var result = sb.ToString();
            File.WriteAllText(config, result);
        }
    }
}
