using System.Diagnostics;
using Addons.Addons;
using Addons.Providers;
using Common.All.Enums;
using Common.Client.Providers;
using Games.Games;
using Microsoft.Extensions.Logging;

namespace Ports.Ports;

public sealed class PortStarter
{
    private readonly PlaytimeProvider _playtimeProvider;
    private readonly InstalledAddonsProviderFactory _installedAddonsProviderFactory;
    private readonly ILogger _logger;

    public PortStarter(
        PlaytimeProvider playtimeProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        ILogger logger
        )
    {
        _playtimeProvider = playtimeProvider;
        _installedAddonsProviderFactory = installedAddonsProviderFactory;
        _logger = logger;

    }

    /// <summary>
    /// Start port
    /// </summary>
    /// <param name="port">Port</param>
    /// <param name="game">Game to start</param>
    /// <param name="addon">Addon to start</param>
    /// <param name="skill">Skill level</param>
    /// <param name="skipIntro">Skip intro</param>
    /// <param name="skipStartup">Skip startup window</param>
    /// <param name="pathToExe">Path to custom port's exe</param>
    public async Task StartAsync(
        BasePort port,
        BaseGame game,
        BaseAddon addon,
        IEnumerable<string> enabledOptions,
        byte? skill,
        bool skipIntro,
        bool skipStartup,
        string? pathToExe = null)
    {
        var sw = Stopwatch.StartNew();

        port.BeforeStart(game, addon);

        var installedAddonsProvider = _installedAddonsProviderFactory.GetSingleton(game);
        var mods = installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);

        var args = port.GetStartGameArgs(game, addon, mods, enabledOptions, skipIntro, skipStartup, skill);

        _ = addon.Executables?[OSEnum.Windows].TryGetValue(port.PortEnum, out pathToExe);

        _logger.LogInformation($"=== Starting addon {addon.AddonId.Id} for {game.FullName} ===");
        _logger.LogInformation($"Path to port exe {pathToExe}");
        _logger.LogInformation($"Startup args:{args}");
        _logger.LogInformation($"Startup args length: {args.Length}");

        await StartPortAsync(port, addon, args, pathToExe).ConfigureAwait(false);

        sw.Stop();
        var time = sw.Elapsed;

        _playtimeProvider.AddTime(addon.AddonId.Id, time);

        port.AfterEnd(game, addon);
    }


    /// <summary>
    /// Start port with command line args
    /// </summary>
    /// <param name="port">Port</param>
    /// <param name="addon">Campaign</param>
    /// <param name="args">Command line arguments</param>
    /// <param name="pathToExe">Path to custom port's exe</param>
    private async Task StartPortAsync(BasePort port, BaseAddon addon, string args, string? pathToExe = null)
    {
        string exe;

        if (pathToExe is not null)
        {
            exe = pathToExe;
        }
        else
        {
            exe = port.PortExeFilePath;
        }

        await Process.Start(new ProcessStartInfo
        {
            FileName = Path.GetFileName(exe),
            UseShellExecute = true,
            Arguments = args,
            WorkingDirectory = Path.GetDirectoryName(exe)
        })!.WaitForExitAsync().ConfigureAwait(false);
    }
}
