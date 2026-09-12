using System.Text;
using Addons.Addons;
using Addons.Helpers;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Core.All.Serializable.Addon;
using Core.Client.Helpers;
using Games.Games;
using Ports.Ports;

namespace Ports.Builders;

/// <summary>
///     Builds command-line parameters for a port.
/// </summary>
public class CmdParametersBuilder
{
    private readonly StringBuilder _sb = new(1000);

    /// <summary>
    ///     Game the arguments are being built for.
    /// </summary>
    protected BaseGame Game { get; }

    /// <summary>
    ///     Addon the arguments are being built for.
    /// </summary>
    protected BaseAddon Addon { get; }

    /// <summary>
    ///     Port the arguments are being built for.
    /// </summary>
    protected BasePort Port { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CmdParametersBuilder" /> class.
    /// </summary>
    public CmdParametersBuilder(BaseGame game, BaseAddon addon, BasePort port)
    {
        Game = game;
        Addon = addon;
        Port = port;
    }

    /// <summary>
    ///     Appends the given string to the command-line parameters.
    /// </summary>
    public CmdParametersBuilder Append(string str)
    {
        _sb.Append(str);

        return this;
    }

    /// <summary>
    ///     Inserts the specified string at the beginning of the command-line arguments being built.
    /// </summary>
    public CmdParametersBuilder AppendStart(string str)
    {
        _sb.Insert(0, str);

        return this;
    }

    /// <summary>
    ///     Appends command-line arguments for enabled options.
    /// </summary>
    public CmdParametersBuilder AppendOptionsArgs(IReadOnlyList<string> enabledOptions)
    {
        if (Addon.Options is null || enabledOptions.Count == 0)
        {
            return this;
        }

        foreach (var optionName in enabledOptions)
        {
            if (!Addon.Options.TryGetValue(optionName, out var options))
            {
                throw new KeyNotFoundException($"Option '{optionName}' not found in addon options.");
            }

            foreach (var option in options)
            {
                if (option.Value is OptionalParameterTypeEnum.DEF)
                {
                    _ = Append($@" {Port.CmdArguments.AddDef}""{option.Key}""");
                }
                else if (option.Value is OptionalParameterTypeEnum.INI && Game is BloodGame)
                {
                    _ = Append($@" -ini ""{option.Key}""");
                }
                else
                {
                    throw new NotSupportedException($"Option '{option.Key}' has unsupported type '{option.Value}' for non-Blood games.");
                }
            }
        }

        return this;
    }

    /// <summary>
    ///     Appends startup arguments for loose maps.
    /// </summary>
    public virtual CmdParametersBuilder AppendLooseMapArgs()
    {
        if (Addon is not LooseMap lMap)
        {
            throw new ArgumentException($"Expected {nameof(MapFileJsonModel)} start map but received {Addon.StartMap?.GetType().Name}.", nameof(Addon));
        }

        if (Addon.StartMap is not MapFileJsonModel mapFile)
        {
            throw new ArgumentException($"Expected {nameof(MapFileJsonModel)} start map but received {Addon.StartMap?.GetType().Name}.", nameof(Addon));
        }

        AppendGameDir(Game);

        if (Game is BloodGame)
        {
            if (lMap.BloodIni is null)
            {
                _ = Append($@" -ini ""{ClientConsts.BloodIni}""");
            }
            else
            {
                _ = Append($@" -ini ""{Path.GetFileName(lMap.BloodIni)}""");
            }
        }

        _ = Append($@" {Port.CmdArguments.AddDirectory}""{Game.MapsFolderPath}""");
        _ = Append($@" -map ""{mapFile.File}""");

        return this;
    }

    /// <summary>
    ///     Appends the command-line parameter for the addon's main def file.
    /// </summary>
    public CmdParametersBuilder AppendMainDefArgs()
    {
        if (Port.CmdArguments.MainDef is null)
        {
            return this;
        }

        if (Addon.MainDef is not null)
        {
            _ = Append($@" {Port.CmdArguments.MainDef}""{Addon.MainDef}""");
        }

        return this;
    }

    /// <summary>
    ///     Appends the command-line parameter to override the default main def file so gamename.def files are ignored.
    /// </summary>
    public CmdParametersBuilder AppendOverrideMainDef()
    {
        if (Port.CmdArguments.MainDef is not null)
        {
            _ = Append($@" {Port.CmdArguments.MainDef}""a""");
        }

        return this;
    }

    /// <summary>
    ///     Appends the command-line parameters for additional def files.
    /// </summary>
    public CmdParametersBuilder AppendAdditionalDefsArgs()
    {
        if (Port.CmdArguments.AddDef is null)
        {
            return this;
        }

        if (Addon.AdditionalDefs is null or [])
        {
            return this;
        }

        foreach (var def in Addon.AdditionalDefs!)
        {
            _ = Append($@" {Port.CmdArguments.AddDef}""{def}""");
        }

        return this;
    }

    /// <summary>
    ///     Appends the command-line parameters for the campaign's main and additional con files.
    /// </summary>
    /// <param name="campaign">
    ///     Duke campaign.
    /// </param>
    protected CmdParametersBuilder AppendConsArgs(DukeCampaign campaign)
    {
        if (Port.CmdArguments.MainCon is not null)
        {
            if (campaign.MainCon is not null)
            {
                _ = Append($@" {Port.CmdArguments.MainCon}""{campaign.MainCon}""");
            }
        }

        if (Port.CmdArguments.AddCon is not null)
        {
            if (campaign.AdditionalCons is not null)
            {
                foreach (var con in campaign.AdditionalCons)
                {
                    _ = Append($@" {Port.CmdArguments.AddCon}""{con}""");
                }
            }
        }

        return this;
    }

    /// <summary>
    ///     Appends the command-line parameter to skip the intro.
    /// </summary>
    public virtual CmdParametersBuilder AppendSkipIntroParameter()
    {
        if (Port.CmdArguments.SkipIntro is not null)
        {
            _ = Append(Port.CmdArguments.SkipIntro!);
        }

        return this;
    }

    /// <summary>
    ///     Appends the command-line parameter to skip the startup window.
    /// </summary>
    public CmdParametersBuilder AppendSkipStartupParameter()
    {
        if (Port.CmdArguments.SkipStartup is not null)
        {
            _ = Append(Port.CmdArguments.SkipStartup!);
        }

        return this;
    }

    /// <summary>
    ///     Appends command-line arguments to load autoload mods.
    /// </summary>
    /// <param name="mods">
    ///     Autoload mods.
    /// </param>
    public CmdParametersBuilder AppendAutoloadModsArgs(IReadOnlyList<BaseAddon> mods)
    {
        if (mods.Count == 0)
        {
            return this;
        }

        var enabledModsCount = 0;

        foreach (var mod in mods)
        {
            if (mod is not AutoloadMod aMod)
            {
                continue;
            }

            if (!AutoloadModsValidator.ValidateAutoloadMod(aMod, Addon, mods, Port.SupportedFeatures))
            {
                continue;
            }

            if (aMod.FileInfo is null)
            {
                continue;
            }

            var aModFileInfo = aMod.FileInfo.Value;

            if (aModFileInfo.IsFolder)
            {
                throw new InvalidOperationException("Folder mods are not supported in autoload");
            }

            _ = Append($@" {Port.CmdArguments.AddFile}""{aModFileInfo.FileName}""");

            if (aMod.AdditionalDefs is not null)
            {
                foreach (var def in aMod.AdditionalDefs)
                {
                    _ = Append($@" {Port.CmdArguments.AddDef}""{def}""");
                }
            }

            if (aMod.AdditionalCons is not null)
            {
                foreach (var con in aMod.AdditionalCons)
                {
                    _ = Append($@" {Port.CmdArguments.AddCon}""{con}""");
                }
            }

            enabledModsCount++;
        }

        if (enabledModsCount > 0 && Port is not Raze)
        {
            _ = Append($@" {Port.CmdArguments.AddDirectory}""{Game.ModsFolderPath}""");
        }

        return this;
    }

    /// <summary>
    ///     Appends the command-line parameter to skip the Steam integration.
    /// </summary>
    public CmdParametersBuilder AppendSkipSteam()
    {
        if (Port.CmdArguments.SkipSteam is not null)
        {
            _ = Append(Port.CmdArguments.SkipSteam);
        }

        return this;
    }

    /// <summary>
    ///     Appends the add-on file path to the command-line parameters.
    /// </summary>
    protected void AppendPathToFile()
    {
        if (Port.CmdArguments.AddFile is not null)
        {
            _ = Append($@" {Port.CmdArguments.AddFile}""{Addon.FileInfo!.Value.PathToFile}""");
        }
    }

    /// <summary>
    ///     Appends command-line arguments for the given game and addon.
    /// </summary>
    /// <param name="game">
    ///     Game the arguments are being built for.
    /// </param>
    /// <param name="addon">
    ///     Addon the arguments are being built for.
    /// </param>
    public CmdParametersBuilder AppendGameArgs(BaseGame game, BaseAddon addon)
    {
        return game switch
        {
            DukeGame dGame => AppendDukeArgs(dGame, addon),
            FuryGame fGame => AppendFuryArgs(fGame, addon),
            NamGame nGame => AppendNamArgs(nGame, (DukeCampaign)addon),
            WW2GIGame gGame => AppendWw2GiArgs(gGame, (DukeCampaign)addon),
            WangGame wGame => AppendWangArgs(wGame, addon),
            RedneckGame rGame => AppendRedneckArgs(rGame, addon),
            WitchavenGame whGame => AppendWitchavenArgs(whGame, addon),
            TekWarGame tGame => AppendTekWarArgs(tGame, addon),
            SlaveGame sGame => AppendSlaveArgs(sGame, (GenericCampaign)addon),
            BloodGame bGame => AppendBloodArgs(bGame, (BloodCampaign)addon),
            _ => throw new NotSupportedException($"Mod type {addon.Type} for game {game} is not supported")
        };
    }

    /// <summary>
    ///     Appends the command-line parameter to add the game install folder to the search path.
    /// </summary>
    protected virtual CmdParametersBuilder AppendGameDir(BaseGame game)
    {
        if (Port.CmdArguments.AddDirectory is not null)
        {
            _ = Append($@" {Port.CmdArguments.AddDirectory}""{game.GameInstallFolder}""");
        }

        return this;
    }

    /// <summary>
    ///     Appends command-line arguments for Slave (PowerSlave) game campaigns.
    /// </summary>
    protected virtual CmdParametersBuilder AppendSlaveArgs(SlaveGame game, GenericCampaign addon)
    {
        AppendGameDir(game);

        if (addon.FileInfo is null)
        {
            return this;
        }

        AppendTcOrMapArgs(addon);

        return this;
    }

    /// <summary>
    ///     Appends command-line arguments for NAM game campaigns.
    /// </summary>
    protected virtual CmdParametersBuilder AppendNamArgs(NamGame game, DukeCampaign addon)
    {
        AppendGameDir(game);

        _ = Append($" -nam {Port.CmdArguments.MainGrp}NAM.GRP");

        if (addon.MainCon is null)
        {
            _ = Append($" {Port.CmdArguments.MainCon}GAME.CON");
        }

        if (addon.FileInfo is null)
        {
            return this;
        }

        AppendConsArgs(addon);

        AppendTcOrMapArgs(addon);


        return this;
    }

    /// <summary>
    ///     Appends command-line arguments for WW2GI game campaigns.
    /// </summary>
    private CmdParametersBuilder AppendWw2GiArgs(WW2GIGame game, DukeCampaign addon)
    {
        AppendGameDir(game);

        _ = Append($" -ww2gi {Port.CmdArguments.MainGrp}WW2GI.GRP");

        if (addon.AddonId.Id.Equals(nameof(WW2GIAddonEnum.Platoon), StringComparison.OrdinalIgnoreCase))
        {
            _ = Append($" {Port.CmdArguments.AddGrp}PLATOONL.DAT {Port.CmdArguments.MainCon}PLATOONL.DEF");
        }
        else if (addon.MainCon is null)
        {
            _ = Append($" {Port.CmdArguments.MainCon}GAME.CON");
        }

        if (addon.FileInfo is null)
        {
            return this;
        }

        AppendConsArgs(addon);

        AppendTcOrMapArgs(addon);

        return this;
    }

    /// <summary>
    ///     Appends command-line arguments for Duke Nukem 3D campaigns.
    /// </summary>
    protected virtual CmdParametersBuilder AppendDukeArgs(DukeGame game, BaseAddon addon) => this;

    /// <summary>
    ///     Appends command-line arguments for Ion Fury games.
    /// </summary>
    protected virtual CmdParametersBuilder AppendFuryArgs(FuryGame game, BaseAddon addon) => this;

    /// <summary>
    ///     Appends command-line arguments for Shadow Warrior games.
    /// </summary>
    protected virtual CmdParametersBuilder AppendWangArgs(WangGame game, BaseAddon addon) => this;

    /// <summary>
    ///     Appends command-line arguments for Redneck Rampage games.
    /// </summary>
    protected virtual CmdParametersBuilder AppendRedneckArgs(RedneckGame game, BaseAddon addon) => this;

    /// <summary>
    ///     Appends command-line arguments for Witchaven games.
    /// </summary>
    protected virtual CmdParametersBuilder AppendWitchavenArgs(WitchavenGame game, BaseAddon addon) => this;

    /// <summary>
    ///     Appends command-line arguments for TekWar games.
    /// </summary>
    protected virtual CmdParametersBuilder AppendTekWarArgs(TekWarGame game, BaseAddon addon) => this;

    /// <summary>
    ///     Appends command-line arguments for Blood game campaigns.
    /// </summary>
    protected virtual CmdParametersBuilder AppendBloodArgs(BloodGame game, BloodCampaign addon)
    {
        AppendGameDir(game);

        if (addon.INI is not null)
        {
            _ = Append($@" -ini ""{addon.INI}""");
        }
        else if (addon.DependentAddons?.ContainsKey(nameof(BloodAddonEnum.BloodCP)) is true)
        {
            _ = Append($@" -ini ""{ClientConsts.CrypticIni}""");
        }

        if (addon.FileInfo is null)
        {
            return this;
        }

        var bCampFileInfo = addon.FileInfo.Value;

        AppendTcOrMapArgs(addon);

        if (addon.RFF is not null)
        {
            _ = Append($@" {Port.CmdArguments.AddRff}""{addon.RFF}""");
        }

        if (addon.SND is not null)
        {
            _ = Append($@" {Port.CmdArguments.AddSnd}""{addon.SND}""");
        }

        return this;
    }

    /// <summary>
    /// Appends command-line arguments for a TC or map addon to the current builder.
    /// </summary>
    /// <param name="addon">The addon whose type determines which command-line arguments are appended.</param>
    /// <exception cref="InvalidOperationException">Thrown when the addon is a total conversion and its file information is unavailable.</exception>
    /// <exception cref="NotSupportedException">Thrown when the addon type is neither a total conversion nor a map.</exception>
    public virtual void AppendTcOrMapArgs(BaseAddon addon)
    {
        if (addon.Type is AddonTypeEnum.TC)
        {
            if (addon.Executables is not null)
            {
                //don't add addon dir if the port is overridden
            }
            else if (addon.FileInfo is null)
            {
                throw new InvalidOperationException("Campaign file info is required.");
            }
            else if (addon.FileInfo.Value.IsFolder)
            {
                _ = Append($@" {Port.CmdArguments.AddGameDir}""{addon.FileInfo.Value.PathToFolder}""");
            }
            else
            {
                _ = Append($@" {Port.CmdArguments.AddDirectory}""{addon.FileInfo.Value.PathToFolder}""");
                _ = Append($@" {Port.CmdArguments.AddFile}""{addon.FileInfo.Value.FileName}""");
            }
        }
        else if (addon.Type is AddonTypeEnum.Map)
        {
            AppendMapArgs(addon);
        }
        else
        {
            throw new NotSupportedException($"Mod type {addon.Type} is not supported");
        }
    }

    /// <summary>
    ///     Appends the command-line arguments for the specified map addon.
    /// </summary>
    /// <param name="addon">
    ///     The map addon.
    /// </param>
    protected void AppendMapArgs(BaseAddon addon)
    {
        if (addon.FileInfo is null)
        {
            throw new InvalidOperationException("Campaign file info is required for map args");
        }

        //TODO e#m#
        if (addon.StartMap is MapFileJsonModel mapFile)
        {
            _ = Append($@" {Port.CmdArguments.AddFile}""{addon.FileInfo.Value.PathToFile}""");
            _ = Append($@" -map ""{mapFile.File}""");
        }
        else
        {
            throw new NotSupportedException($"Unsupported start map type: {addon.StartMap?.GetType().Name}.");
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _sb.ToString();
    }
}
