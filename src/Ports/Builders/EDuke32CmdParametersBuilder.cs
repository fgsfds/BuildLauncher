using Addons.Addons;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Core.All.Enums.Versions;
using Core.Client.Helpers;
using Games.Games;
using Ports.Ports;

namespace Ports.Builders;

/// <summary>
///     Command parameters builder for the EDuke32 port and its derivatives.
/// </summary>
public class EDuke32CmdParametersBuilder : CmdParametersBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EDuke32CmdParametersBuilder" /> class.
    /// </summary>
    public EDuke32CmdParametersBuilder(BaseGame game, BaseAddon addon, BasePort port)
        : base(game, addon, port)
    {
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendDukeArgs(DukeGame game, BaseAddon addon)
    {
        if (addon.SupportedGame.GameEnum is GameEnum.Duke64)
        {
            _ = Append(@$" {Port.CmdArguments.AddDirectory}""{Path.GetDirectoryName(game.Duke64RomPath)}"" {Port.CmdArguments.MainGrp}""{Path.GetFileName(game.Duke64RomPath)}""");

            return this;
        }

        if (addon.SupportedGame.GameVersion?.Equals(nameof(DukeVersionEnum.Duke3D_WT), StringComparison.OrdinalIgnoreCase) == true)
        {
            _ = Append($@" {Port.CmdArguments.AddDirectory}""{game.DukeWTInstallPath}"" -addon {(byte)DukeAddonEnum.Base} {Port.CmdArguments.AddDirectory}""{Path.Combine(Port.InstallFolderPath, ClientConsts.WTStopgap)}"" {Port.CmdArguments.MainGrp}e32wt.grp {Port.CmdArguments.AddDef}e32wt.def");
        }
        else
        {
            _ = Append($@" {Port.CmdArguments.AddDirectory}""{game.GameInstallFolder}""");

            if (addon.DependentAddons is not null)
            {
                //DUKE IT OUT IN DC
                if (addon.DependentAddons.ContainsKey(nameof(DukeAddonEnum.DukeDC)))
                {
                    var addonPath = game.AddonsFolders[DukeAddonEnum.DukeDC];

                    if (!addonPath.Equals(game.GameInstallFolder))
                    {
                        _ = Append($@" {Port.CmdArguments.AddDirectory}""{addonPath}""");
                    }

                    _ = Append($" {Port.CmdArguments.AddGrp}DUKEDC.GRP");

                    if (File.Exists(Path.Combine(addonPath, "DUKEDC.CON")))
                    {
                        _ = Append($" {Port.CmdArguments.MainCon}DUKEDC.CON");
                    }
                }
                //NUCLEAR WINTER
                else if (addon.DependentAddons.ContainsKey(nameof(DukeAddonEnum.DukeNW)))
                {
                    var addonPath = game.AddonsFolders[DukeAddonEnum.DukeNW];

                    if (!addonPath.Equals(game.GameInstallFolder))
                    {
                        _ = Append($@" {Port.CmdArguments.AddDirectory}""{addonPath}""");
                    }

                    _ = Append($" {Port.CmdArguments.AddGrp}NWINTER.GRP {Port.CmdArguments.MainCon}NWINTER.CON");
                }
                //CARIBBEAN
                else if (addon.DependentAddons.ContainsKey(nameof(DukeAddonEnum.DukeVaca)))
                {
                    var addonPath = game.AddonsFolders[DukeAddonEnum.DukeVaca];

                    if (!addonPath.Equals(game.GameInstallFolder))
                    {
                        _ = Append($@" {Port.CmdArguments.AddDirectory}""{addonPath}""");
                    }

                    _ = Append($" {Port.CmdArguments.AddGrp}VACATION.GRP");

                    if (File.Exists(Path.Combine(addonPath, "VACATION.CON")))
                    {
                        _ = Append($" {Port.CmdArguments.MainCon}VACATION.CON");
                    }
                }
            }
        }

        if (addon.FileInfo is null)
        {
            return this;
        }

        if (addon is LooseMap)
        {
            AppendLooseMapArgs();

            return this;
        }

        if (addon is not DukeCampaign dCamp)
        {
            throw new ArgumentException($"Expected {nameof(DukeCampaign)} but received {addon.GetType().Name}.", nameof(addon));
        }

        AppendConsArgs(dCamp);

        AppendTcOrMapArgs(addon);

        return this;
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendFuryArgs(FuryGame game, BaseAddon addon)
    {
        AppendGameDir(game);

        if (addon.FileInfo is null)
        {
            return this;
        }

        if (addon is LooseMap)
        {
            AppendLooseMapArgs();
            return this;
        }

        if (addon is not DukeCampaign fCamp)
        {
            throw new ArgumentException($"Expected {nameof(DukeCampaign)} but received {addon.GetType().Name}.", nameof(addon));
        }

        AppendConsArgs(fCamp);

        AppendTcOrMapArgs(addon);

        return this;
    }
}
