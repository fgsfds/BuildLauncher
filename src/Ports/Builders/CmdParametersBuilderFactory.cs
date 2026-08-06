using Addons.Addons;
using Core.All.Enums;
using Games.Games;
using Ports.Ports;

namespace Ports.Builders;

/// <summary>
///     Creates <see cref="CmdParametersBuilder" /> instances for the given port.
/// </summary>
public static class CmdParametersBuilderFactory
{
    /// <summary>
    ///     Creates a command parameters builder appropriate for the given port.
    /// </summary>
    public static CmdParametersBuilder Create(BaseGame game, BaseAddon addon, BasePort port)
    {
        switch (port.PortEnum)
        {
            case PortEnum.VoidSW:
                return new VoidSWCmdParametersBuilder(game, addon, port);
            case PortEnum.RedNukem:
                return new RedNukemCmdParametersBuilder(game, addon, port);
            case PortEnum.EDuke32:
            case PortEnum.NBlood:
            case PortEnum.NotBlood:
            case PortEnum.PCExhumed:
                return new EDuke32CmdParametersBuilder(game, addon, port);
            case PortEnum.Fury:
                return new FuryCmdParametersBuilder(game, addon, port);
            case PortEnum.Raze:
                return new RazeCmdParametersBuilder(game, addon, port);
            case PortEnum.DosBox:
                return new DosBoxCmdParametersBuilder(game, addon, port);
            case PortEnum.BuildGDX:
                return new BuildGDXCmdParametersBuilder(game, addon, port);
            default:
                return new CmdParametersBuilder(game, addon, port);
        }
    }
}
