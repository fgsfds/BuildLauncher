using Common.All.Enums;
using Common.All.Serializable.Downloadable;
using Common.Client;
using Common.Client.Interfaces;
using Common.Client.Tools;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Tools.Tools;

namespace Tools.Installer;

public sealed class ToolInstaller : InstallerBase<BaseTool>
{
    private readonly InstalledGamesProvider _gamesProvider;
    private readonly IApiInterface _apiInterface;

    public ToolInstaller(
        BaseTool tool,
        IApiInterface apiInterface,
        InstalledGamesProvider gamesProvider,
        FilesDownloader filesDownloader,
        ArchiveTools archiveTools,
        ILogger logger
        ) : base(tool, filesDownloader, archiveTools)
    {
        _gamesProvider = gamesProvider;
        _apiInterface = apiInterface;
    }

    /// <inheritdoc/>
    protected override void InstallInternal(string filePath)
    {
        if (_instance.ToolEnum is ToolEnum.Mapster32)
        {
            throw new Exception($"{nameof(ToolEnum.Mapster32)} can't be installed separately.");
        }
        else if (_instance.ToolEnum is ToolEnum.XMapEdit)
        {
            var pathToBlood = _gamesProvider.GetGame(GameEnum.Blood).GameInstallFolder ?? throw new Exception();

            _ = Directory.CreateDirectory(_instance.InstallFolderPath);

            var files = Directory.GetFiles(pathToBlood);
            foreach (var file in files)
            {
                if (!file.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase)
                    && !file.EndsWith(".ogg", StringComparison.CurrentCultureIgnoreCase)
                    && !file.EndsWith(".txt", StringComparison.CurrentCultureIgnoreCase))
                {
                    string fileName = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(_instance.InstallFolderPath, fileName), true);
                }
            }
        }
        else if (_instance.ToolEnum is ToolEnum.DOSBlood)
        {
            var pathToBlood = _gamesProvider.GetGame(GameEnum.Blood).GameInstallFolder ?? throw new Exception();
            var bloodExe = Path.Combine(pathToBlood, "BLOOD.EXE");
            var bloodExeBak = bloodExe + ".BAK";

            if (!File.Exists(bloodExeBak))
            {
                File.Move(bloodExe, bloodExe + ".BAK", true);
            }
        }
    }

    /// <inheritdoc/>
    public override void Uninstall()
    {
        if (_instance.ToolEnum is ToolEnum.Mapster32)
        {
            throw new Exception($"{nameof(ToolEnum.Mapster32)} can't be uninstalled separately.");
        }
        else if (_instance.ToolEnum is ToolEnum.DOSBlood)
        {
            var game = _gamesProvider.GetGame(GameEnum.Blood);
            if (game.GameInstallFolder is null)
            {
                throw new Exception();
            }
            var bloodExe = Path.Combine(game.GameInstallFolder, "BLOOD.EXE");

            File.Delete(bloodExe);
            File.Move(bloodExe + ".BAK", bloodExe);
        }
        else
        {
            Directory.Delete(_instance.InstallFolderPath, true);
        }
    }

    /// <inheritdoc/>
    public override Task<GeneralReleaseJsonModel?> GetRelease() => _apiInterface.GetLatestToolReleaseAsync(_instance.ToolEnum);
}
