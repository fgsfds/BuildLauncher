using Common.All.Enums;
using Games.Providers;

namespace Tools.Tools;

public sealed class XMapEdit : BaseTool
{
    private readonly InstalledGamesProvider _gamesProvider;

    /// <inheritdoc/>
    protected override string WinExe => "xmapedit.exe";

    /// <inheritdoc/>
    protected override string LinExe => throw new NotSupportedException();

    /// <inheritdoc/>
    public override string Name => "XMAPEDIT";

    /// <inheritdoc/>
    public override ToolEnum ToolEnum => ToolEnum.XMapEdit;

    /// <inheritdoc/>
    public override bool CanBeInstalled => _gamesProvider.IsBloodInstalled;

    /// <inheritdoc/>
    public override bool CanBeLaunched => true;

    public XMapEdit(InstalledGamesProvider gamesProvider)
    {
        _gamesProvider = gamesProvider;
    }

    /// <inheritdoc/>
    public override string GetStartToolArgs() => string.Empty;
}
