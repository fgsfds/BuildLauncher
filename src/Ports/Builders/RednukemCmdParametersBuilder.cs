using Addons.Addons;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Games.Games;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Ports.Builders;

/// <summary>
///     Command parameters builder for the Rednukem port.
/// </summary>
public sealed class RednukemCmdParametersBuilder : EDuke32CmdParametersBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RednukemCmdParametersBuilder" /> class.
    /// </summary>
    public RednukemCmdParametersBuilder(BaseGame game, BaseAddon addon, BasePort port)
        : base(game, addon, port)
    {
    }

    /// <inheritdoc />
    public override CmdParametersBuilder AppendSkipIntroParameter()
    {
        base.AppendSkipIntroParameter();

        _ = Append(" -d blank.edm");

        if (Port is not Rednukem rednukem)
        {
            throw new Exception();
        }

        rednukem.CreateOrDeleteBlankAnm(false);

        return this;
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendRedneckArgs(RedneckGame game, BaseAddon addon)
    {
        if (addon.SupportedGame.GameEnum is GameEnum.RidesAgain)
        {
            _ = Append($@" {Port.CmdArguments.AddDirectory}""{game.AgainInstallPath}""");
        }
        else if (addon.DependentAddons?.ContainsKey(nameof(RedneckAddonEnum.Route66)) is true)
        {
            _ = Append($@" {Port.CmdArguments.AddDirectory}""{game.GameInstallFolder}"" -x GAME66.CON");
        }
        else
        {
            _ = Append($@" {Port.CmdArguments.AddDirectory}""{game.GameInstallFolder}""");
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

        if (addon is not DukeCampaign rCamp)
        {
            throw new ArgumentException($"Expected {nameof(DukeCampaign)} but received {addon.GetType().Name}.", nameof(addon));
        }

        AppendConsArgs(rCamp);

        AppendTcOrMapArgs(addon);

        return this;
    }
}
