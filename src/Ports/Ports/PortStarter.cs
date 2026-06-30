using System.Diagnostics;
using Addons.Addons;
using Addons.Providers;
using Core.All.Enums;
using Core.Client.Providers;
using Games.Games;
using Microsoft.Extensions.Logging;

namespace Ports.Ports;

public sealed class PortStarter
{
    private readonly InstalledAddonsProviderFactory _installedAddonsProviderFactory;
    private readonly ILogger<PortStarter> _logger;
    private readonly PlaytimeProvider _playtimeProvider;

    public PortStarter(
        PlaytimeProvider playtimeProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        ILogger<PortStarter> logger
        )
    {
        _playtimeProvider = playtimeProvider;
        _installedAddonsProviderFactory = installedAddonsProviderFactory;
        _logger = logger;
    }

    /// <summary>
    ///     Start port
    /// </summary>
    /// <param name="port">Port</param>
    /// <param name="game">Game to start</param>
    /// <param name="addon">Addon to start</param>
    /// <param name="enabledOptions">List of enabled options</param>
    /// <param name="skill">Skill level</param>
    /// <param name="skipIntro">Skip intro</param>
    /// <param name="skipStartup">Skip startup window</param>
    /// <param name="pathToExe">Path to custom port's exe</param>
    public async Task StartAsync(
        BasePort port,
        BaseGame game,
        BaseAddon addon,
        IReadOnlyList<string> enabledOptions,
        byte? skill,
        bool skipIntro,
        bool skipStartup,
        string? pathToExe = null
        )
    {
        var sw = Stopwatch.StartNew();

        port.BeforeStart(game, addon);

        var installedAddonsProvider = _installedAddonsProviderFactory.Get(game);
        var mods = installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);

        var args = port.GetStartGameArgs(game, addon, mods, enabledOptions, skipIntro, skipStartup, skill);

        if (addon.Executables?.TryGetValue(OSEnum.Windows, out var winDict) is true)
        {
            winDict.TryGetValue(port.PortEnum, out pathToExe);
        }

        _logger.LogInformation($"=== Starting addon {addon.AddonId.Id} for {game.FullName} ===");
        _logger.LogInformation($"Path to port exe {pathToExe}");
        _logger.LogInformation($"Startup args:{args}");
        _logger.LogInformation($"Startup args length: {args.Length}");

        await StartPortAsync(port, args, pathToExe).ConfigureAwait(false);

        sw.Stop();
        var time = sw.Elapsed;

        _playtimeProvider.AddTime(addon.AddonId.Id, time);

        port.AfterEnd(game, addon);
    }


    /// <summary>
    ///     Start port with command line args
    /// </summary>
    /// <param name="port">Port</param>
    /// <param name="args">Command line arguments</param>
    /// <param name="pathToExe">Path to custom port's exe</param>
    private async Task StartPortAsync(BasePort port, string args, string? pathToExe = null)
    {
        var exe = pathToExe ?? port.PortExeFilePath;

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = Path.GetFileName(exe),
            UseShellExecute = true,
            Arguments = args,
            WorkingDirectory = Path.GetDirectoryName(exe)
        });

        if (process is null)
        {
            _logger.LogError("Failed to start process: {Exe}", exe);

            return;
        }

        await process.WaitForExitAsync().ConfigureAwait(false);
    }
}
