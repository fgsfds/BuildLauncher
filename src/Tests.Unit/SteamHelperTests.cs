using Core.Client.Helpers;

namespace Tests.Unit;

/// <summary> Verifies Steam discovery with isolated native, XDG and Flatpak-style layouts. </summary>
public sealed class SteamHelperTests : IDisposable
{
    /// <summary> The private fixture root. </summary>
    private readonly string _root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    /// <inheritdoc />
    public void Dispose()
    {
        if (Directory.Exists(_root)) { Directory.Delete(_root, true); }
    }

    /// <summary> Recognizes Linux Steam layouts without accessing the real home directory. </summary>
    /// <param name="layout">The fixture layout.</param>
    [Theory]
    [InlineData("native")]
    [InlineData("legacy")]
    [InlineData("flatpak")]
    [InlineData("xdg")]
    public void FindLinuxSteamInstallPath_RecognizesLayout(string layout)
    {
        var xdg = Path.Combine(_root, "custom data");
        var expected = layout switch
        {
            "native" => Path.Combine(_root, ".local", "share", "Steam"),
            "legacy" => Path.Combine(_root, ".steam", "steam"),
            "flatpak" => Path.Combine(_root, ".var", "app", "com.valvesoftware.Steam", ".local", "share", "Steam"),
            _ => Path.Combine(xdg, "Steam")
        };
        _ = Directory.CreateDirectory(expected);
        Assert.Equal(expected, SteamHelper.FindLinuxSteamInstallPath(_root, layout == "xdg" ? xdg : null));
    }

    /// <summary> Keeps the default library without an index and discovers paths containing spaces from an index. </summary>
    [Fact]
    public void GetSteamLibraries_IncludesDefaultAndAdditionalLibraries()
    {
        var primary = Path.Combine(_root, "Steam", "steamapps", "common");
        var secondary = Path.Combine(_root, "other library");
        _ = Directory.CreateDirectory(primary);
        _ = Directory.CreateDirectory(Path.Combine(secondary, "steamapps", "common"));
        Assert.Equal([primary], SteamHelper.GetSteamLibraries(Path.Combine(_root, "Steam")));
        var escaped = secondary.Replace(Path.DirectorySeparatorChar, '/');
        var quote = '"';
        File.WriteAllText(Path.Combine(_root, "Steam", "steamapps", "libraryfolders.vdf"), $"{quote}path{quote} {quote}{escaped}{quote}");
        Assert.Equal([primary, Path.Combine(secondary, "steamapps", "common")], SteamHelper.GetSteamLibraries(Path.Combine(_root, "Steam")));
    }
}
