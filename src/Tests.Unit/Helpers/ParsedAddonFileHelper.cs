using Core.All.Enums;
using Core.All.Serializable.Addon;
using Core.Client.Helpers;

namespace Tests.Unit.Helpers;

/// <summary>
///     Helper for creating test <see cref="ParsedAddonFile" /> instances.
/// </summary>
public static class ParsedAddonFileHelper
{
    /// <summary>
    ///     Creates a <see cref="ParsedAddonFile" /> with the given identity and a dummy file path.
    /// </summary>
    public static ParsedAddonFile CreateParsedAddonFile(
        string id,
        string title,
        string version,
        AddonTypeEnum addonType,
        GameEnum game = GameEnum.Duke3D
        )
    {
        return new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(PathHelper.GetFakePath(), "addon.json"),
            SupportedGame = game,
            Manifest = new AddonManifestJsonModel
            {
                Id = id,
                Title = title,
                Version = version,
                AddonType = addonType,
                SupportedGame = new SupportedGameJsonModel
                {
                    Game = game
                }
            },
            GridHash = null,
            PreviewHash = null
        };
    }

    /// <summary>
    ///     Creates a <see cref="ParsedAddonFile" /> with a Mod type, optional dependencies, and incompatibles.
    /// </summary>
    public static ParsedAddonFile CreateParsedModFile(
        string id,
        string title,
        string version,
        GameEnum game = GameEnum.Duke3D,
        Dictionary<string, string?>? deps = null,
        Dictionary<string, string?>? incompatibles = null)
    {
        return new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(PathHelper.GetFakePath(), "addon.json"),
            SupportedGame = game,
            Manifest = new AddonManifestJsonModel
            {
                Id = id,
                Title = title,
                Version = version,
                AddonType = AddonTypeEnum.Mod,
                Author = "test author",
                Description = "test description",
                SupportedGame = new SupportedGameJsonModel
                {
                    Game = game
                },
                Dependencies = deps is not null
                    ? new DependencyJsonModel
                    {
                        Addons = deps.Select(d => new DependantAddonJsonModel
                            {
                                Id = d.Key,
                                Version = d.Value
                            }
                            ).ToList()
                    }
                    : null,
                Incompatibles = incompatibles is not null
                    ? new DependencyJsonModel
                    {
                        Addons = incompatibles.Select(d => new DependantAddonJsonModel
                            {
                                Id = d.Key,
                                Version = d.Value
                            }
                            ).ToList()
                    }
                    : null
            },
            GridHash = null,
            PreviewHash = null
        };
    }
}
