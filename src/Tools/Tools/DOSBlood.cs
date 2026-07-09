using Core.All.Enums;
using Games.Providers;

namespace Tools.Tools;

/// <summary>
///     DOSBlood tool implementation.
/// </summary>
public sealed class DOSBlood : BaseTool
{
    private readonly InstalledGamesProvider _gamesProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DOSBlood" /> class.
    /// </summary>
    public DOSBlood(InstalledGamesProvider gamesProvider)
    {
        _gamesProvider = gamesProvider;
    }

    /// <inheritdoc />
    protected override string WinExe => "BLOOD.EXE";

    /// <inheritdoc />
    protected override string LinExe => throw new NotSupportedException();

    /// <inheritdoc />
    public override string Name => "DOSBlood";

    /// <inheritdoc />
    public override ToolEnum ToolEnum => ToolEnum.DOSBlood;

    /// <inheritdoc />
    public override string InstallFolderPath => _gamesProvider.GetGame(GameEnum.Blood).GameInstallFolder ?? string.Empty;

    /// <inheritdoc />
    public override bool CanBeInstalled => _gamesProvider.GetGame(GameEnum.Blood).IsBaseGameInstalled;

    /// <inheritdoc />
    public override bool CanBeLaunched => false;

    /// <inheritdoc />
    public override string InstallText => string.Empty;

    /// <inheritdoc />
    public override string? InstalledVersion
    {
        get
        {
            if (string.IsNullOrWhiteSpace(InstallFolderPath))
            {
                return null;
            }

            var bloodExe = Path.Combine(InstallFolderPath, "BLOOD.EXE");

            if (!File.Exists(bloodExe))
            {
                return null;
            }

            var bloodExeInfo = new FileInfo(bloodExe);

            if (bloodExeInfo.Length == 1442615)
            {
                return null;
            }

            var versionFile = Path.Combine(InstallFolderPath, "version");

            if (!File.Exists(versionFile))
            {
                return null;
            }

            try
            {
                return File.ReadAllText(versionFile);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <inheritdoc />
    public override string GetStartToolArgs() => throw new NotSupportedException();
}
