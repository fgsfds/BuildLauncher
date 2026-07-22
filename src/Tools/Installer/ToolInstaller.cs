using Core.All.Enums;
using Core.All.Serializable.Downloadable;
using Core.Client;
using Core.Client.Interfaces;
using Core.Client.Tools;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Tools.Tools;

namespace Tools.Installer;

/// <summary>
///     Handles installation and uninstallation of tools.
/// </summary>
public sealed class ToolInstaller : InstallerBase<BaseTool>
{
    private readonly IApiInterface _apiInterface;

    private readonly InstalledGamesProvider _gamesProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ToolInstaller" /> class.
    /// </summary>
    public ToolInstaller(
        BaseTool tool,
        IApiInterface apiInterface,
        InstalledGamesProvider gamesProvider,
        FilesDownloader filesDownloader,
        ArchiveTools archiveTools,
        ILogger<ToolInstaller> logger
        ) : base(tool, filesDownloader, archiveTools, logger)
    {
        _gamesProvider = gamesProvider;
        _apiInterface = apiInterface;
    }

    /// <inheritdoc />
    protected override void PostInstall(string filePath)
    {
        if (_instance.ToolEnum is ToolEnum.Mapster32)
        {
            throw new InvalidOperationException($"{nameof(ToolEnum.Mapster32)} can't be installed separately.");
        }
        else if (_instance.ToolEnum is ToolEnum.XMapEdit)
        {
            var pathToBlood = _gamesProvider.GetGame(GameEnum.Blood).GameInstallFolder ?? throw new InvalidOperationException("Can't find Blood install path.");

            _ = Directory.CreateDirectory(_instance.InstallFolderPath);

            var files = Directory.GetFiles(pathToBlood);

            foreach (var file in files)
            {
                if (file.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase)
                 || file.EndsWith(".ogg", StringComparison.InvariantCultureIgnoreCase)
                 || file.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase)
                 || file.EndsWith("version", StringComparison.InvariantCultureIgnoreCase)
                   )
                {
                    continue;
                }

                var fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(_instance.InstallFolderPath, fileName), true);
            }
        }
    }

    /// <inheritdoc />
    protected override void Backup()
    {
        if (_instance.ToolEnum is ToolEnum.DOSBlood)
        {
            var pathToBlood = _gamesProvider.GetGame(GameEnum.Blood).GameInstallFolder ?? throw new InvalidOperationException("Can't find Blood install path.");
            var bloodExe = Path.Combine(pathToBlood, "BLOOD.EXE");
            var bloodExeBak = bloodExe + ".BAK";

            if (!File.Exists(bloodExeBak))
            {
                File.Move(bloodExe, bloodExe + ".BAK", true);
            }
        }
    }

    /// <inheritdoc />
    public override void Uninstall()
    {
        if (_instance.ToolEnum is ToolEnum.Mapster32)
        {
            throw new InvalidOperationException($"{nameof(ToolEnum.Mapster32)} can't be uninstalled separately.");
        }
        else if (_instance.ToolEnum is ToolEnum.DOSBlood)
        {
            var game = _gamesProvider.GetGame(GameEnum.Blood);

            if (game.GameInstallFolder is null)
            {
                throw new InvalidOperationException("Can't find Blood install path.");
            }

            var bloodExe = Path.Combine(game.GameInstallFolder, "BLOOD.EXE");
            var versionFile = Path.Combine(game.GameInstallFolder, "version");

            File.Delete(bloodExe);
            File.Delete(versionFile);
            File.Move(bloodExe + ".BAK", bloodExe);
        }
        else
        {
            Directory.Delete(_instance.InstallFolderPath, true);
        }
    }

    /// <inheritdoc />
    public override Task<GeneralReleaseJsonModel?> GetRelease(CancellationToken cancellationToken = default) => _apiInterface.GetLatestToolReleaseAsync(_instance.ToolEnum, cancellationToken);
}
