using Core.All.Enums;
using Games.Providers;

namespace Tools.Tools;

/// <summary>
///     XMapEdit tool implementation.
/// </summary>
public sealed class XMapEdit : BaseTool
{
    private readonly InstalledGamesProvider _gamesProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="XMapEdit" /> class.
    /// </summary>
    public XMapEdit(InstalledGamesProvider gamesProvider)
    {
        _gamesProvider = gamesProvider;
    }

    /// <inheritdoc />
    protected override string WinExe => "xmapedit.exe";

    /// <inheritdoc />
    protected override string LinExe => throw new NotSupportedException();

    /// <inheritdoc />
    public override string Name => "XMAPEDIT";

    /// <inheritdoc />
    public override ToolEnum ToolEnum => ToolEnum.XMapEdit;

    /// <inheritdoc />
    public override bool CanBeInstalled => _gamesProvider.IsBloodInstalled;

    /// <inheritdoc />
    public override bool CanBeLaunched => true;

    /// <inheritdoc />
    public override string GetStartToolArgs() => string.Empty;
}
