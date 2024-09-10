using Common.Enums;
using Common.Helpers;
using Common.Releases;

namespace Common.Providers;

public sealed class RepositoriesProvider
{
    public RepositoryEntity GetPortRepo(PortEnum portEnum)
    {
        if (portEnum is PortEnum.BuildGDX)
        {
            return new()
            {
                RepoUrl = new($"{Consts.FilesRepo}/Ports/BuildGDX_v117.zip"),
                WindowsReleasePredicate = null,
                LinuxReleasePredicate = null,
            };
        }
        else if (portEnum is PortEnum.Raze)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/ZDoom/Raze/releases"),
                WindowsReleasePredicate = static x => x.FileName.EndsWith(".zip", StringComparison.CurrentCultureIgnoreCase) && x.FileName.Contains("windows", StringComparison.CurrentCultureIgnoreCase),
                LinuxReleasePredicate = null,
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
                WindowsReleasePredicate = static x => x.FileName.StartsWith("nblood_win64", StringComparison.CurrentCultureIgnoreCase),
                LinuxReleasePredicate = null,
            };
        }
        else if (portEnum is PortEnum.NotBlood)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/clipmove/NotBlood/releases"),
                WindowsReleasePredicate = static x => x.FileName.StartsWith("notblood-win64", StringComparison.CurrentCultureIgnoreCase),
                LinuxReleasePredicate = null,
            };
        }
        else if (portEnum is PortEnum.PCExhumed)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/nukeykt/NBlood/releases"),
                WindowsReleasePredicate = static x => x.FileName.StartsWith("pcexhumed_win64", StringComparison.CurrentCultureIgnoreCase),
                LinuxReleasePredicate = null,
            };
        }
        else if (portEnum is PortEnum.RedNukem)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/nukeykt/NBlood/releases"),
                WindowsReleasePredicate = static x => x.FileName.StartsWith("rednukem_win64", StringComparison.CurrentCultureIgnoreCase),
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
        else
        {
            return ThrowHelper.NotImplementedException<RepositoryEntity>(portEnum.ToString());
        }
    }

    public RepositoryEntity GetToolRepo(ToolEnum toolEnum)
    {
        if (toolEnum is ToolEnum.XMapEdit)
        {
            return new()
            {
                RepoUrl = new("https://api.github.com/repos/NoOneBlood/xmapedit/releases"),
                WindowsReleasePredicate = static x => x.FileName.EndsWith("x64.zip", StringComparison.InvariantCultureIgnoreCase),
                LinuxReleasePredicate = null
            };
        }
        else if (toolEnum is ToolEnum.Mapster32)
        {
            return new()
            {
                RepoUrl = null,
                WindowsReleasePredicate = null,
                LinuxReleasePredicate = null
            };
        }
        else
        {
            return ThrowHelper.NotImplementedException<RepositoryEntity>(toolEnum.ToString());
        }
    }
}

public readonly struct RepositoryEntity
{
    public required Uri? RepoUrl { get; init; }
    public required Func<GitHubReleaseAsset, bool>? WindowsReleasePredicate { get; init; }
    public required Func<GitHubReleaseAsset, bool>? LinuxReleasePredicate { get; init; }
}
