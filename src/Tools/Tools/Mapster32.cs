using Core.All.Enums;
using Core.Client.Helpers;
using Games.Providers;

namespace Tools.Tools;

/// <summary>
///     Mapster32 tool implementation.
/// </summary>
public sealed class Mapster32 : BaseTool
{
    private readonly InstalledGamesProvider _gamesProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Mapster32" /> class.
    /// </summary>
    public Mapster32(InstalledGamesProvider gamesProvider)
    {
        _gamesProvider = gamesProvider;
    }

    /// <inheritdoc />
    protected override string WinExe => "mapster32.exe";

    /// <inheritdoc />
    protected override string LinExe => throw new NotSupportedException();

    /// <inheritdoc />
    public override string Name => "Mapster32";

    /// <inheritdoc />
    public override ToolEnum ToolEnum => ToolEnum.Mapster32;

    /// <inheritdoc />
    public override string InstallFolderPath => Path.Combine(ClientProperties.PortsFolderPath, "EDuke32");

    /// <inheritdoc />
    public override bool CanBeInstalled => false;

    /// <inheritdoc />
    public override bool CanBeLaunched => _gamesProvider.GetGame(GameEnum.Duke3D).IsBaseGameInstalled;

    /// <inheritdoc />
    public override string InstallText => IsInstalled ? " " : "Install EDuke32 from Ports tab";

    /// <inheritdoc />
    public override string GetStartToolArgs()
    {
        var game = _gamesProvider.GetGame(GameEnum.Duke3D);

        if (!game.IsBaseGameInstalled)
        {
            throw new NotSupportedException();
        }

        return $@"-game_dir ""{game.GameInstallFolder}""";
    }
}
