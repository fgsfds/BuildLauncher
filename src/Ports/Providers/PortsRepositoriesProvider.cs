using Common.Common.Serializable.Downloadable;
using Common.Enums;
using CommunityToolkit.Diagnostics;

namespace Ports.Providers;

internal static class PortsRepositoriesProvider
{
    public static RepositoryEntity GetPortRepo(PortEnum portEnum)
    {
        if (portEnum is PortEnum.BuildGDX)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/fgsfds/BuildGDX-Releases/releases"),
                WindowsReleasePredicate = static x => x.FileName.EndsWith("windows.zip", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = null,
            };
        }
        else if (portEnum is PortEnum.Raze)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/ZDoom/Raze/releases"),
                WindowsReleasePredicate = static x => x.FileName.EndsWith("windows.zip", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = static x => x.FileName.EndsWith("linux-portable.tar.xz", StringComparison.OrdinalIgnoreCase)
            };
        }
        else if (portEnum is PortEnum.EDuke32 or PortEnum.VoidSW)
        {
            return new()
            {
                RepoUrl = new("https://dukeworld.com/eduke32/synthesis/latest/"),
                WindowsReleasePredicate = null,
                LinuxReleasePredicate = null,
            };
        }
        else if (portEnum is PortEnum.NBlood)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/nukeykt/NBlood/releases"),
                WindowsReleasePredicate = static x => x.FileName.StartsWith("nblood_win64", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = null,
            };
        }
        else if (portEnum is PortEnum.NotBlood)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/clipmove/NotBlood/releases"),
                WindowsReleasePredicate = static x => x.FileName.EndsWith("win64.zip", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = static x => x.FileName.EndsWith("linux-clang.zip", StringComparison.OrdinalIgnoreCase)
            };
        }
        else if (portEnum is PortEnum.PCExhumed)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/nukeykt/NBlood/releases"),
                WindowsReleasePredicate = static x => x.FileName.StartsWith("pcexhumed_win64", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = null,
            };
        }
        else if (portEnum is PortEnum.RedNukem)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/nukeykt/NBlood/releases"),
                WindowsReleasePredicate = static x => x.FileName.StartsWith("rednukem_win64", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = null,
            };
        }
        else if (portEnum is PortEnum.Fury)
        {
            return new()
            {
                RepoUrl = null,
                WindowsReleasePredicate = null,
                LinuxReleasePredicate = null,
            };
        }
        else if (portEnum is PortEnum.DosBox)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/dosbox-staging/dosbox-staging/releases"),
                WindowsReleasePredicate = static x => x.FileName.StartsWith("dosbox-staging-windows-x64", StringComparison.OrdinalIgnoreCase) && x.FileName.EndsWith("zip", StringComparison.OrdinalIgnoreCase),
                LinuxReleasePredicate = null,
            };
        }
        else
        {
            return ThrowHelper.ThrowNotSupportedException<RepositoryEntity>(portEnum.ToString());
        }
    }
}

internal readonly struct RepositoryEntity
{
    public required Uri? RepoUrl { get; init; }
    public required Func<GitHubReleaseAsset, bool>? WindowsReleasePredicate { get; init; }
    public required Func<GitHubReleaseAsset, bool>? LinuxReleasePredicate { get; init; }
}
