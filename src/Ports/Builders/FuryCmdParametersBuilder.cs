using Addons.Addons;
using Games.Games;
using Ports.Ports;

namespace Ports.Builders;

/// <summary>
///     Command parameters builder for the Fury port.
/// </summary>
public sealed class FuryCmdParametersBuilder : EDuke32CmdParametersBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FuryCmdParametersBuilder" /> class.
    /// </summary>
    public FuryCmdParametersBuilder(BaseGame game, BaseAddon addon, BasePort port)
        : base(game, addon, port)
    {
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendGameDir(BaseGame game)
    {
        return this;
    }
}
