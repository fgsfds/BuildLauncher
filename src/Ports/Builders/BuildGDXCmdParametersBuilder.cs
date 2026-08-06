using Addons.Addons;
using Core.All.Enums;
using Core.All.Enums.Versions;
using Games.Games;
using Ports.Ports;

namespace Ports.Builders;

/// <summary>
///     Command parameters builder for the BuildGDX port.
/// </summary>
public sealed class BuildGDXCmdParametersBuilder : CmdParametersBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BuildGDXCmdParametersBuilder" /> class.
    /// </summary>
    public BuildGDXCmdParametersBuilder(BaseGame game, BaseAddon addon, BasePort port)
        : base(game, addon, port)
    {
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendDukeArgs(DukeGame game, BaseAddon camp)
    {
        if (camp.AddonId.Id.Equals(nameof(DukeVersionEnum.Duke3D_WT), StringComparison.OrdinalIgnoreCase))
        {
            _ = Append($@" -path ""{game.DukeWTInstallPath}""");
        }
        else
        {
            _ = Append($@" -path ""{game.GameInstallFolder}""");
        }

        _ = Append(" -game DUKE_NUKEM_3D");

        return this;
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendBloodArgs(BloodGame game, BloodCampaign addon)
    {
        _ = Append($@" -path ""{game.GameInstallFolder}""");
        _ = Append(" -game BLOOD");

        return this;
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendWangArgs(WangGame game, BaseAddon camp)
    {
        _ = Append($@" -path ""{game.GameInstallFolder}""");
        _ = Append(" -game SHADOW_WARRIOR");

        return this;
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendRedneckArgs(RedneckGame game, BaseAddon camp)
    {
        if (camp.AddonId.Id.Equals(nameof(GameEnum.RidesAgain), StringComparison.OrdinalIgnoreCase))
        {
            _ = Append($@" -path ""{game.AgainInstallPath}""");
            _ = Append(" -game RR_RIDES_AGAIN");
        }
        else
        {
            _ = Append($@" -path ""{game.GameInstallFolder}""");
            _ = Append(" -game REDNECK_RAMPAGE");
        }

        return this;
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendSlaveArgs(SlaveGame game, GenericCampaign addon)
    {
        _ = Append($@" -path ""{game.GameInstallFolder}""");
        _ = Append(" -game POWERSLAVE");

        return this;
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendNamArgs(NamGame game, DukeCampaign addon)
    {
        _ = Append($@" -path ""{game.GameInstallFolder}""");
        _ = Append(" -game NAM");

        return this;
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendWitchavenArgs(WitchavenGame game, BaseAddon addon)
    {
        if (addon.AddonId.Id.Equals(nameof(GameEnum.Witchaven2), StringComparison.OrdinalIgnoreCase))
        {
            _ = Append($@" -path ""{game.Witchaven2InstallPath}""");
            _ = Append(" -game WITCHAVEN_2");
        }
        else
        {
            _ = Append($@" -path ""{game.GameInstallFolder}""");
            _ = Append(" -game WITCHAVEN");
        }

        return this;
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendTekWarArgs(TekWarGame game, BaseAddon addon)
    {
        _ = Append($@" -path ""{game.GameInstallFolder}""");
        _ = Append(" -game TEKWAR");

        return this;
    }
}
