﻿using Common.All.Enums;
using Common.Client.Interfaces;
using Common.Client.Tools;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Tools.Providers;

namespace Tools.Installer;

public sealed class ToolsInstallerFactory(
    IApiInterface apiInterface,
    InstalledGamesProvider gamesProvider,
    HttpClient httpClient,
    ILogger logger,
    InstalledToolsProvider toolsProvider
    )
{
    /// <summary>
    /// Create <see cref="ToolsInstaller"/> instance
    /// </summary>
    public ToolsInstaller Create(ToolEnum toolEnum) => new(toolEnum, apiInterface, gamesProvider, httpClient, toolsProvider, logger);
}

public sealed class ToolsInstaller
{
    private readonly ToolEnum _toolEnum;
    private readonly IApiInterface _apiInterface;
    private readonly ArchiveTools _fileTools;
    private readonly InstalledGamesProvider _gamesProvider;
    private readonly InstalledToolsProvider _toolsProvider;

    public ToolsInstaller(
        ToolEnum toolEnum,
        IApiInterface apiInterface,
        InstalledGamesProvider gamesProvider,
        HttpClient httpClient,
        InstalledToolsProvider toolsProvider,
        ILogger logger
        )
    {
        _toolEnum = toolEnum;
        _toolsProvider = toolsProvider;
        _fileTools = new(httpClient, logger);
        _apiInterface = apiInterface;
        _gamesProvider = gamesProvider;
        Progress = _fileTools.Progress;
    }

    /// <summary>
    /// Installation progress
    /// </summary>
    public Progress<float> Progress { get; init; }

    /// <summary>
    /// Install tool
    /// </summary>
    public async Task InstallAsync()
    {
        var tool = _toolsProvider.GetTool(_toolEnum);

        var release = await _apiInterface.GetLatestToolReleaseAsync(tool.ToolEnum).ConfigureAwait(false);

        if (release?.DownloadUrl is null)
        {
            return;
        }

        var filePath = Path.GetFileName(release.DownloadUrl.ToString());

        _ = await _fileTools.DownloadFileAsync(release.DownloadUrl, filePath, CancellationToken.None).ConfigureAwait(false);

        if (tool.ToolEnum is ToolEnum.XMapEdit)
        {
            var pathToBlood = _gamesProvider.GetGame(GameEnum.Blood).GameInstallFolder ?? throw new Exception();

            _ = Directory.CreateDirectory(tool.ToolInstallFolderPath);

            var files = Directory.GetFiles(pathToBlood);
            foreach (var file in files)
            {
                if (!file.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase)
                    && !file.EndsWith(".ogg", StringComparison.CurrentCultureIgnoreCase)
                    && !file.EndsWith(".txt", StringComparison.CurrentCultureIgnoreCase))
                {
                    string fileName = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(tool.ToolInstallFolderPath, fileName), true);
                }
            }
        }

        if (tool.ToolEnum is ToolEnum.DOSBlood)
        {
            var pathToBlood = _gamesProvider.GetGame(GameEnum.Blood).GameInstallFolder ?? throw new Exception();
            var bloodExe = Path.Combine(pathToBlood, "BLOOD.EXE");
            var bloodExeBak = bloodExe + ".BAK";

            if (!File.Exists(bloodExeBak))
            {
                File.Move(bloodExe, bloodExe + ".BAK", true);
            }
        }

        await _fileTools.UnpackArchiveAsync(filePath, tool.ToolInstallFolderPath).ConfigureAwait(false);

        File.Delete(filePath);

        File.WriteAllText(Path.Combine(tool.ToolInstallFolderPath, "version"), release.Version);
    }

    public void Uninstall()
    {
        var tool = _toolsProvider.GetTool(_toolEnum);

        if (tool.ToolEnum is ToolEnum.Mapster32)
        {
            throw new Exception();
        }
        else if (tool.ToolEnum is ToolEnum.DOSBlood)
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
            Directory.Delete(tool.ToolInstallFolderPath, true);
        }

    }
}
