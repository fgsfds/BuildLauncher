using Addons.Providers;
using Common.Client.Helpers;
using Common.Client.Providers;
using Common.Common.Helpers;
using Common.Enums;
using Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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
    /// <returns></returns>
    public async Task StartAsync(BasePort port, IGame game, IAddon addon, byte? skill, bool skipIntro, bool skipStartup)
    {
        var sw = Stopwatch.StartNew();

        port.BeforeStart(game, addon);

        var installedAddonsProvider = _installedAddonsProviderFactory.GetSingleton(game);
        var mods = installedAddonsProvider.GetInstalledMods();

        var args = port.GetStartGameArgs(game, addon, mods, skipIntro, skipStartup, skill);

        _logger.LogInformation($"=== Starting addon {addon.Id} for {game.FullName} ===");
        _logger.LogInformation($"Path to port exe {addon.Executables?[OSEnum.Windows] ?? port.PortExeFilePath}");
        _logger.LogInformation($"Startup args:{args}");
        _logger.LogInformation($"Startup args length: {args.Length}");

        await StartPortAsync(port, addon, args).ConfigureAwait(false);

        sw.Stop();
        var time = sw.Elapsed;

        _playtimeProvider.AddTime(addon.Id, time);

        port.AfterEnd(game, addon);
    }


    /// <summary>
    /// Start port with command line args
    /// </summary>
    /// <param name="port">Port</param>
    /// <param name="addon">Campaign</param>
    /// <param name="args">Command line arguments</param>
    private async Task StartPortAsync(BasePort port, IAddon addon, string args)
    {
        var portExe = addon.Executables?[CommonProperties.OSEnum] is not null ? addon.Executables[CommonProperties.OSEnum] : port.PortExeFilePath;

        await Process.Start(new ProcessStartInfo
        {
            FileName = Path.GetFileName(portExe),
            UseShellExecute = true,
            Arguments = args,
            WorkingDirectory = Path.GetDirectoryName(portExe)
        })!.WaitForExitAsync().ConfigureAwait(false);
    }
}
