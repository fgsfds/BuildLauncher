using System.Diagnostics;
using Addons.Addons;
using Addons.Providers;
using Core.All;
using Core.All.Enums;
using Core.All.Helpers;
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
    /// <returns>The launch, playtime persistence, and cleanup outcome.</returns>
    public async Task<Result> StartAsync(
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
        var executable = ResolveExecutable(port, addon, CommonProperties.OSEnum, pathToExe);
        var prepared = false;
        var result = new Result(ResultEnum.Success, string.Empty);
        try
        {
            port.BeforeStart(game, addon);
            prepared = true;
            var installedAddonsProvider = _installedAddonsProviderFactory.Get(game);
            var mods = installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
            var args = port.GetStartGameArgs(game, addon, mods, enabledOptions, skipIntro, skipStartup, skill);

            _logger.LogInformation("Starting addon {Addon} with {Executable}. Arguments: {Arguments}", addon.AddonId.Id, executable, args);
            using var process = _processRunner.Start(Path.GetFileName(executable), args, Path.GetDirectoryName(executable)!);
            if (process is null)
            {
                throw new InvalidOperationException("The operating system did not return a process handle.");
            }

            var stopwatch = Stopwatch.StartNew();
            await _processRunner.WaitForExitAsync(process).ConfigureAwait(false);
            stopwatch.Stop();
            _playtimeProvider.AddTime(addon.AddonId.Id, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run {Executable}.", executable);
            result = new Result(ResultEnum.Error, $"Could not run {executable}: {ex.Message}");
        }
        finally
        {
            if (prepared)
            {
                try
                {
                    port.AfterEnd(game, addon);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cleanup failed for {Executable}.", executable);
                    var message = $"Could not restore game files after running {executable}: {ex.Message}";
                    result = new Result(ResultEnum.Error, result.IsSuccess ? message : result.Message + Environment.NewLine + message);
                }
            }
        }

        return result;
    }

    /// <summary> Resolves an explicit user choice before an OS-specific addon override and the default port. </summary>
    /// <param name="port">The selected port.</param>
    /// <param name="addon">The selected addon.</param>
    /// <param name="os">The runtime operating system.</param>
    /// <param name="customExecutable">The explicit user-selected executable, if any.</param>
    /// <returns>The executable to launch.</returns>
    public static string ResolveExecutable(BasePort port, BaseAddon addon, OSEnum os, string? customExecutable = null)
    {
        if (!string.IsNullOrWhiteSpace(customExecutable))
        {
            return customExecutable;
        }

        if (addon.Executables?.TryGetValue(os, out var executables) is true &&
            executables.TryGetValue(port.PortEnum, out var executable) && !string.IsNullOrWhiteSpace(executable))
        {
            return executable;
        }

        return port.PortExeFilePath;
    }
}
