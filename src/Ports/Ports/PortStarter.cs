using System.Diagnostics;
using Addons.Providers;
using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Interfaces;
using Common.Client.Providers;
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
    public async Task StartAsync(BasePort port, IGame game, IAddon addon, byte? skill, bool skipIntro, bool skipStartup, string? pathToExe = null)
    {
        var sw = Stopwatch.StartNew();

        port.BeforeStart(game, addon);

        var installedAddonsProvider = _installedAddonsProviderFactory.GetSingleton(game);
        var mods = installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);

        var args = port.GetStartGameArgs(game, addon, mods, skipIntro, skipStartup, skill);

        _logger.LogInformation($"=== Starting addon {addon.AddonId.Id} for {game.FullName} ===");
        _logger.LogInformation($"Path to port exe {addon.Executables?[OSEnum.Windows] ?? port.PortExeFilePath}");
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
    private async Task StartPortAsync(BasePort port, IAddon addon, string args, string? pathToExe = null)
    {
        string exe;

        if (pathToExe is not null)
        {
            exe = pathToExe;
        }
        else if (addon.Executables?[CommonProperties.OSEnum] is not null)
        {
            exe = addon.Executables[CommonProperties.OSEnum];
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
