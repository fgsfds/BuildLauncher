using Addons.Addons;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Core.All.Enums.Versions;
using Games.Games;
using Ports.Ports;

namespace Ports.Builders;

/// <summary>
///     Command parameters builder for the Raze port.
/// </summary>
public sealed class RazeCmdParametersBuilder : CmdParametersBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RazeCmdParametersBuilder" /> class.
    /// </summary>
    public RazeCmdParametersBuilder(BaseGame game, BaseAddon addon, BasePort port)
        : base(game, addon, port)
    {
    }

    /// <summary>
    ///     Raze locates game files via its config file, so the game install folder is not added to the command line.
    /// </summary>
    protected override CmdParametersBuilder AppendGameDir(BaseGame game)
    {
        return this;
    }

    /// <inheritdoc />
    public override void AppendTcOrMapArgs(BaseAddon addon)
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
                _ = Append($@" {Port.CmdArguments.AddFile}""{addon.FileInfo.Value.PathToFolder}""");
            }
            else
            {
                //Raze locates the addon folder via its config file, so only the file name goes on the command line.
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

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendDukeArgs(DukeGame game, BaseAddon addon)
    {
        if (addon is LooseMap)
        {
            AppendLooseMapArgs();
            return this;
        }

        if (addon is not DukeCampaign dCamp)
        {
            throw new ArgumentException($"Expected {nameof(DukeCampaign)} but received {addon.GetType().Name}.", nameof(addon));
        }

        if (dCamp.SupportedGame.GameVersion is not null &&
            dCamp.SupportedGame.GameVersion.Equals(nameof(DukeVersionEnum.Duke3D_WT), StringComparison.OrdinalIgnoreCase))
        {
            ArgumentNullException.ThrowIfNull(game.DukeWTInstallPath);
            Raze.AddGamePathsToConfig(game, addon, game.DukeWTInstallPath, Port.PortConfigFilePath);

            _ = Append($" -addon {(byte)DukeAddonEnum.Base}");
        }
        else
        {
            var dukeAddon = (byte)DukeAddonEnum.Base;

            if (dCamp.DependentAddons is null)
            {
                dukeAddon = (byte)DukeAddonEnum.Base;
            }
            else if (dCamp.DependentAddons.ContainsKey(nameof(DukeAddonEnum.DukeDC)))
            {
                dukeAddon = (byte)DukeAddonEnum.DukeDC;
            }
            else if (dCamp.DependentAddons.ContainsKey(nameof(DukeAddonEnum.DukeNW)))
            {
                dukeAddon = (byte)DukeAddonEnum.DukeNW;
            }
            else if (dCamp.DependentAddons.ContainsKey(nameof(DukeAddonEnum.DukeVaca)))
            {
                dukeAddon = (byte)DukeAddonEnum.DukeVaca;
            }

            _ = Append($" -addon {dukeAddon}");
        }

        if (dCamp.FileInfo is null)
        {
            return this;
        }

        AppendConsArgs(dCamp);

                AppendTcOrMapArgs(addon);

        return this;
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendWangArgs(WangGame game, BaseAddon addon)
    {
        if (addon is LooseMap)
        {
            AppendLooseMapArgs();
            return this;
        }

        if (addon is not GenericCampaign wCamp)
        {
            throw new ArgumentException($"Expected {nameof(GenericCampaign)} but received {addon.GetType().Name}.", nameof(addon));
        }

        //TODO downloaded addons support
        if (wCamp.DependentAddons is not null &&
            wCamp.DependentAddons.ContainsKey(nameof(WangAddonEnum.Wanton)))
        {
            _ = Append($" {Port.CmdArguments.AddFile}WT.GRP");
        }
        else if (wCamp.DependentAddons is not null &&
                 wCamp.DependentAddons.ContainsKey(nameof(WangAddonEnum.TwinDragon)))
        {
            _ = Append($" {Port.CmdArguments.AddFile}TD.GRP");
        }

        if (wCamp.FileInfo is null)
        {
            return this;
        }

        if (wCamp.FileInfo.Value.IsFolder)
        {
            _ = Append($@" {Port.CmdArguments.AddFile}""{wCamp.FileInfo.Value.PathToFolder}""");
        }
        else
        {
            //Raze locates the addon folder via its config file, so only the file name goes on the command line.
            _ = Append($@" {Port.CmdArguments.AddFile}""{wCamp.FileInfo.Value.FileName}""");
        }

        return this;
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendRedneckArgs(RedneckGame game, BaseAddon addon)
    {
        if (addon is LooseMap)
        {
            AppendLooseMapArgs();
            return this;
        }

        if (addon is not DukeCampaign rCamp)
        {
            throw new ArgumentException($"Expected {nameof(DukeCampaign)} but received {addon.GetType().Name}.", nameof(addon));
        }

        if (rCamp.DependentAddons is not null &&
            rCamp.DependentAddons.ContainsKey(nameof(RedneckAddonEnum.Route66)))
        {
            _ = Append(" -route66");

            return this;
        }

        if (rCamp.SupportedGame.GameEnum is GameEnum.RidesAgain)
        {
            ArgumentNullException.ThrowIfNull(game.AgainInstallPath);
            Raze.AddGamePathsToConfig(game, addon, game.AgainInstallPath, Port.PortConfigFilePath);
        }

        if (rCamp.FileInfo is null)
        {
            return this;
        }

        AppendConsArgs(rCamp);

        AppendTcOrMapArgs(addon);

        return this;
    }
}
