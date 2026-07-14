using System.Diagnostics;
using Addons.Addons;
using Addons.Providers;
using Core.All.Enums;
using Core.Client.Interfaces;
using Core.Client.Providers;
using Games.Games;
using Microsoft.Extensions.Logging;

namespace Ports.Ports;

/// <summary>
///     Starts a game port with the appropriate command-line arguments.
/// </summary>
public sealed class PortStarter
{
    private readonly InstalledAddonsProviderFactory _installedAddonsProviderFactory;

    private readonly ILogger<PortStarter> _logger;

    private readonly PlaytimeProvider _playtimeProvider;

    private readonly IProcessRunner _processRunner;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PortStarter" /> class.
    /// </summary>
    /// <param name="playtimeProvider">Playtime tracking provider.</param>
    /// <param name="installedAddonsProviderFactory">Factory for installed addons providers.</param>
    /// <param name="processRunner">Abstraction for starting and waiting for processes.</param>
    /// <param name="logger">Logger instance.</param>
    public PortStarter(
        PlaytimeProvider playtimeProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        IProcessRunner processRunner,
        ILogger<PortStarter> logger
        )
    {
        _playtimeProvider = playtimeProvider;
        _installedAddonsProviderFactory = installedAddonsProviderFactory;
        _processRunner = processRunner;
        _logger = logger;
    }

    /// <summary>
    ///     Starts the specified port for a given game and addon.
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
    ///     Starts the port executable asynchronously.
    /// </summary>
    /// <param name="port">The port to start.</param>
    /// <param name="args">Command-line arguments.</param>
    /// <param name="pathToExe">Optional custom path to the executable.</param>
    private async Task StartPortAsync(BasePort port, string args, string? pathToExe = null)
    {
        var exe = pathToExe ?? port.PortExeFilePath;

        try
        {
            var process = _processRunner.Start(Path.GetFileName(exe), args, Path.GetDirectoryName(exe)!);

            if (process is null)
            {
                _logger.LogError("Failed to start process: {Exe}", exe);

                return;
            }

            await _processRunner.WaitForExitAsync(process).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error while starting port {Port}", port.Name);
        }
    }
}
