using System.Diagnostics.CodeAnalysis;
using Addons.Addons;
using Core.All;
using Core.All.Enums;

namespace Addons.Providers;

/// <summary>
///     Provides methods for parsing GRP info files and creating addon entries from the associated .grp files.
/// </summary>
public static class GrpInfoProvider
{
    /// <summary>
    ///     Attempts to retrieve addons from a GRP info file by parsing the associated .grp files.
    /// </summary>
    /// <param name="pathToGrpInfo">Path to the GRP info file containing metadata about game groups.</param>
    /// <param name="newAddons">When this method returns true, contains a list of BaseAddon instances created from the parsed .grp files; otherwise null.</param>
    /// <returns>true if addons were successfully retrieved; false if no .grp files were found.</returns>
    public static bool TryGetAddonsFromGrpInfo(string pathToGrpInfo, [NotNullWhen(true)] out List<BaseAddon>? newAddons)
    {
        var grpInfoFolder = Path.GetDirectoryName(pathToGrpInfo) ?? throw new InvalidOperationException($"Could not determine directory for {pathToGrpInfo}");

        var grps = Directory.GetFiles(grpInfoFolder, "*.grp", SearchOption.TopDirectoryOnly);

        if (grps.Length == 0)
        {
            newAddons = null;

            return false;
        }

        var grpInfos = Parse(pathToGrpInfo, grps.Length);
        newAddons = new(grps.Length);

        foreach (var grp in grps)
        {
            var fileSize = new FileInfo(grp).Length;

            var grpInfo = grpInfos.FirstOrDefault(x => x.Size == fileSize);

            if (grpInfo.Equals(default(GrpInfoEntry)) || grpInfo.Name is null)
            {
                continue;
            }

            AddonId addonId = new(grpInfo.Name.ToLower().Replace(" ", "_"), null);

            DukeCampaign camp = new()
            {
                AddonId = addonId,
                Type = AddonTypeEnum.TC,
                SupportedGame = new(GameEnum.Duke3D),
                Title = grpInfo.Name,
                GridImageHash = null,
                PreviewImageHash = null,
                Description = null,
                Author = null,
                ReleaseDate = null,
                FileInfo = new(grpInfoFolder, Path.GetFileName(grp)),
                DependentAddons = null,
                IncompatibleAddons = null,
                StartMap = null,
                MainCon = grpInfo.MainCon,
                AdditionalCons = null,
                MainDef = null,
                AdditionalDefs = grpInfo.AddDef is null ? null : [grpInfo.AddDef],
                RTS = null,
                RequiredFeatures = [FeatureEnum.EDuke32_CON],
                Executables = null,
                IsFavorite = false,
                Options = null
            };

            newAddons.Add(camp);
        }

        return true;
    }

    /// <summary>
    ///     Parse grpinfo file
    /// </summary>
    /// <param name="pathToFile">Path to the grpinfo file</param>
    /// <param name="expectedGrpsCount">Number of expected grps</param>
    internal static List<GrpInfoEntry> Parse(string pathToFile, int expectedGrpsCount = 10)
    {
        var lines = File.ReadLines(pathToFile);

        List<GrpInfoEntry> addons = new(expectedGrpsCount);

        string? name = null;
        string? mainCon = null;
        string? def = null;
        var size = 0;

        var isInsideGrpInfoBlock = false;

        foreach (var line in lines)
        {
            var trimmed = line.AsSpan().Trim();

            if (trimmed.IsEmpty)
            {
                continue;
            }

            if (trimmed.StartsWith('{'))
            {
                name = null;
                mainCon = null;
                def = null;
                size = 0;

                isInsideGrpInfoBlock = true;

                continue;
            }

            if (!isInsideGrpInfoBlock)
            {
                continue;
            }

            if (trimmed.StartsWith("name"))
            {
                name = ExtractQuotedValue(trimmed);
            }
            else if (trimmed.StartsWith("scriptname"))
            {
                mainCon = ExtractQuotedValue(trimmed);
            }
            else if (trimmed.StartsWith("defname"))
            {
                def = ExtractQuotedValue(trimmed);
            }
            else if (trimmed.StartsWith("size"))
            {
                var sizePart = trimmed["size".Length..].Trim();
                size = int.Parse(sizePart);
            }
            else if (trimmed.StartsWith('}'))
            {
                if (!string.IsNullOrWhiteSpace(name) && size > 0)
                {
                    GrpInfoEntry addon = new()
                    {
                        Name = name,
                        MainCon = mainCon,
                        AddDef = def,
                        Size = size
                    };

                    addons.Add(addon);
                }

                isInsideGrpInfoBlock = false;
            }
        }

        return addons;
    }

    /// <summary>
    ///     Extracts the value between the first and last quote characters from the given span.
    /// </summary>
    /// <param name="span">The span to extract the quoted value from.</param>
    /// <returns>The extracted value, or null if no quoted value is found.</returns>
    private static string? ExtractQuotedValue(ReadOnlySpan<char> span)
    {
        var pFrom = span.IndexOf('"') + 1;
        var pTo = span.LastIndexOf('"');

        if (pFrom > 0 && pTo > pFrom)
        {
            return span[pFrom..pTo].ToString();
        }

        return null;
    }
}


/// <summary>
///     Represents a single entry parsed from a GRP info file.
/// </summary>
public readonly struct GrpInfoEntry
{
    /// <summary>
    ///     Name of the addon.
    /// </summary>
    public readonly string Name { get; init; }

    /// <summary>
    ///     Main CON script file name, if any.
    /// </summary>
    public readonly string? MainCon { get; init; }

    /// <summary>
    ///     Additional DEF file name, if any.
    /// </summary>
    public readonly string? AddDef { get; init; }

    /// <summary>
    ///     Size of the matching .grp file in bytes.
    /// </summary>
    public readonly int Size { get; init; }
}
