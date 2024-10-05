using Common;
using Common.Enums;
using Common.Interfaces;

namespace Games.Games;

public sealed class StandaloneGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.Standalone;

    /// <inheritdoc/>
    public override string FullName => "Standalone";

    /// <inheritdoc/>
    public override string ShortName => FullName;

    /// <inheritdoc/>
    public override List<string> RequiredFiles => [];

    /// <inheritdoc/>
    public override Dictionary<AddonVersion, IAddon> GetOriginalCampaigns() => [];
}
