using Common.Enums;
using Common.Interfaces;

namespace Ports.Ports.EDuke32;

/// <summary>
/// NotBlood port
/// </summary>
public sealed class NotBlood : NBlood
{
    /// <inheritdoc/>
    public override PortEnum PortEnum => PortEnum.NotBlood;

    /// <inheritdoc/>
    protected override string WinExe => "notblood.exe";

    /// <inheritdoc/>
    protected override string LinExe => "notblood";

    /// <inheritdoc/>
    public override string Name => "NotBlood";

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames => [GameEnum.Blood];

    /// <inheritdoc/>
    public override List<FeatureEnum> SupportedFeatures =>
        [
        FeatureEnum.Modern_Types,
        FeatureEnum.Hightile,
        FeatureEnum.Models,
        FeatureEnum.TileFromTexture
        ];

    /// <inheritdoc/>
    protected override string ConfigFile => "notblood.cfg";


    /// <inheritdoc/>
    public override void BeforeStart(IGame game, IAddon campaign)
    {
        MoveSaveFilesFromGameFolder(game, campaign);
    }
}
