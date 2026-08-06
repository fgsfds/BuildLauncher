using Addons.Addons;
using Core.All.Enums.Addons;
using Games.Games;
using Ports.Ports;

namespace Ports.Builders;

/// <summary>
///     Command parameters builder for the VoidSW port.
/// </summary>
public sealed class VoidSWCmdParametersBuilder : EDuke32CmdParametersBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="VoidSWCmdParametersBuilder" /> class.
    /// </summary>
    public VoidSWCmdParametersBuilder(BaseGame game, BaseAddon addon, BasePort port)
        : base(game, addon, port)
    {
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendWangArgs(WangGame game, BaseAddon addon)
    {
        AppendGameDir(game);

        if (addon is LooseMap)
        {
            AppendLooseMapArgs();
            return this;
        }

        if (addon is not GenericCampaign wCamp)
        {
            throw new ArgumentException($"Expected {nameof(GenericCampaign)} but received {addon.GetType().Name}.", nameof(addon));
        }

        if (wCamp.DependentAddons?.ContainsKey(nameof(WangAddonEnum.Wanton)) == true)
        {
            _ = Append($" -addon{(byte)WangAddonEnum.Wanton}");
        }
        else if (wCamp.DependentAddons?.ContainsKey(nameof(WangAddonEnum.TwinDragon)) == true)
        {
            _ = Append($" -addon{(byte)WangAddonEnum.TwinDragon}");
        }
        else
        {
            _ = Append($" -addon{(byte)WangAddonEnum.Base}");
        }

        AddWangMusicFolder(game);

        if (wCamp.FileInfo is null)
        {
            return this;
        }

        AppendTcOrMapArgs(addon);

        return this;
    }

    /// <summary>
    ///     Adds the music folder to the command-line arguments if the game uses MIDI music.
    /// </summary>
    private void AddWangMusicFolder(WangGame game)
    {
        if (game.GameInstallFolder is null)
        {
            return;
        }

        if (File.Exists(Path.Combine(game.GameInstallFolder, "track02.ogg")))
        {
            return;
        }

        var folder = Path.Combine(game.GameInstallFolder, "MUSIC");

        if (Directory.Exists(folder))
        {
            _ = Append(@$" -j""{folder}""");

            return;
        }

        folder = Path.Combine(game.GameInstallFolder, "classic", "MUSIC");

        if (Directory.Exists(folder))
        {
            _ = Append(@$" -j""{folder}""");
        }
    }
}
