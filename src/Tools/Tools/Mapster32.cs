using Common.Client.Helpers;
using Common.Enums;
using CommunityToolkit.Diagnostics;
using Games.Providers;

namespace Tools.Tools;

public sealed class Mapster32 : BaseTool
{
    private readonly InstalledGamesProvider _gamesProvider;

    /// <inheritdoc/>
    public override string Exe => "mapster32.exe";

    /// <inheritdoc/>
    public override string Name => "Mapster32";

    /// <inheritdoc/>
    public override ToolEnum ToolEnum => ToolEnum.Mapster32;

    /// <inheritdoc/>
    public override string PathToExecutableFolder => Path.Combine(ClientProperties.PortsFolderPath, "EDuke32");

    /// <inheritdoc/>
    public override bool CanBeInstalled => false;

    /// <inheritdoc/>
    public override bool CanBeLaunched => _gamesProvider.GetGame(GameEnum.Duke3D).IsBaseGameInstalled;

    public override string InstallText => "Install EDuke32\nfrom Ports tab";

    public Mapster32(InstalledGamesProvider gamesProvider)
    {
        _gamesProvider = gamesProvider;
    }


    /// <inheritdoc/>
    public override string GetStartToolArgs()
    {
        var game = _gamesProvider.GetGame(GameEnum.Duke3D);

        if (!game.IsBaseGameInstalled)
        {
            ThrowHelper.ThrowNotSupportedException();
        }

        return $@"-game_dir ""{game.GameInstallFolder}""";
    }
}
