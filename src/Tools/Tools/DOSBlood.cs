using Common.All.Enums;
using Games.Providers;

namespace Tools.Tools;

public sealed class DOSBlood : BaseTool
{
    private readonly InstalledGamesProvider _gamesProvider;

    /// <inheritdoc/>
    protected override string WinExe => "BLOOD.EXE";

    /// <inheritdoc/>
    protected override string LinExe => throw new NotSupportedException();

    /// <inheritdoc/>
    public override string Name => "DOSBlood";

    /// <inheritdoc/>
    public override ToolEnum ToolEnum => ToolEnum.DOSBlood;

    /// <inheritdoc/>
    public override string InstallFolderPath => _gamesProvider.GetGame(GameEnum.Blood).GameInstallFolder ?? string.Empty;

    /// <inheritdoc/>
    public override bool CanBeInstalled => _gamesProvider.GetGame(GameEnum.Blood).IsBaseGameInstalled;

    /// <inheritdoc/>
    public override bool CanBeLaunched => false;

    public override string InstallText => string.Empty;

    /// <summary>
    /// Currently installed version
    /// </summary>
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

            return File.ReadAllText(versionFile);
        }
    }

    public DOSBlood(InstalledGamesProvider gamesProvider)
    {
        _gamesProvider = gamesProvider;
    }

    /// <inheritdoc/>
    public override string GetStartToolArgs() => throw new NotSupportedException();
}
