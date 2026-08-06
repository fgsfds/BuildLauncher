using System.Collections.Immutable;
using Addons.Addons;
using Core.All.Enums;
using Core.Client.Interfaces;
using Games.Games;
using Ports.Builders;
using Ports.Helpers;

namespace Ports.Ports.EDuke32;

/// <summary>
///     Fury port.
/// </summary>
public sealed class Fury : EDuke32
{
    private readonly IConfigProvider _config;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Fury" /> class.
    /// </summary>
    /// <param name="config">
    ///     Configuration provider.
    /// </param>
    public Fury(IConfigProvider config)
    {
        _config = config;
    }

    /// <inheritdoc />
    public override PortEnum PortEnum => PortEnum.Fury;

    /// <inheritdoc />
    protected override string WinExe => "fury.exe";

    /// <inheritdoc />
    protected override string LinExe => throw new NotSupportedException();

    /// <inheritdoc />
    public override string Name => "Fury";

    /// <inheritdoc />
    public override ImmutableHashSet<GameEnum> SupportedGames { get; } = [GameEnum.Fury];

    /// <inheritdoc />
    public override string InstallFolderPath => _config.PathFury ?? string.Empty;

    /// <inheritdoc />
    public override bool IsInstalled => File.Exists(PortExeFilePath);

    /// <inheritdoc />
    public override ImmutableHashSet<FeatureEnum> SupportedFeatures { get; } =
    [
        FeatureEnum.EDuke32_CON,
        FeatureEnum.Dynamic_Lighting,
        FeatureEnum.Hightile,
        FeatureEnum.Models,
        FeatureEnum.Sloped_Sprites,
        FeatureEnum.TROR,
        FeatureEnum.Wall_Rotate_Cstat,
        FeatureEnum.TileFromTexture
    ];

    /// <inheritdoc />
    protected override string ConfigFile => "fury.cfg";

    /// <inheritdoc />
    public override bool IsDownloadable => false;


    /// <inheritdoc />
    public override void BeforeStart(BaseGame game, BaseAddon campaign)
    {
        SaveFilesHelper.MoveSaveFilesFromStorage(
            GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
            GetGameSaveFilesFolder(game, campaign));
        FixConfig();
    }


    /// <inheritdoc />
    protected override void CustomModifyArgs(CmdParametersBuilder sb, BaseGame game, BaseAddon addon)
    {
    }
}
