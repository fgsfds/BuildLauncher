using System.Diagnostics.CodeAnalysis;
using Addons.Addons;
using Core.All;
using Core.All.Enums;
using Core.All.Enums.Versions;

namespace Addons.Providers;

public static class GrpInfoProvider
{
    /// <summary>
    /// Get list of addons from grpinfo files located in the folder and its subfolders.
    /// </summary>
    public static List<BaseAddon>? GetAddonsFromGrpInfo(string pathToFolder)
    {
        List<BaseAddon> newAddons = [];

        var grpInfos = Directory.GetFiles(pathToFolder, "*.grpinfo", SearchOption.AllDirectories);

        if (grpInfos.Length == 0)
        {
            return null;
        }

        foreach (var grpInfo in grpInfos)
        {
            if (TryGetAddonsFromGrpInfo(grpInfo, out var foundAddons))
            {
                newAddons.AddRange(foundAddons);
            }
        }

        return newAddons;
    }

    private static bool TryGetAddonsFromGrpInfo(string pathToGrpInfo, [NotNullWhen(true)] out List<BaseAddon>? newAddons)
    {
        var grpInfoFolder = Path.GetDirectoryName(pathToGrpInfo) ?? throw new InvalidOperationException();

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

            AddonId version = new(grpInfo.Name.ToLower().Replace(" ", "_"), null);

            DukeCampaign camp = new()
            {
                AddonId = version,
                Type = AddonTypeEnum.TC,
                SupportedGame = new(GameEnum.Duke3D),
                Title = grpInfo.Name,
                GridImageHash = null,
                PreviewImageHash = null,
                Description = null,
                Author = null,
                ReleaseDate = null,
                PathToFile = grp,
                DependentAddons = null,
                IncompatibleAddons = null,
                StartMap = null,
                MainCon = grpInfo.MainCon,
                AdditionalCons = null,
                MainDef = null,
                AdditionalDefs = grpInfo.AddDef is null ? null : [grpInfo.AddDef],
                RTS = null,
                RequiredFeatures = [FeatureEnum.EDuke32_CON],
                IsUnpacked = false,
                Executables = null,
                IsFavorite = false,
                Options = null
            };

            newAddons.Add(camp);
        }

        return true;
    }

    /// <summary>
    /// Parse grpinfo file
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

        var isInsideGrpinfoBlock = false;

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

                isInsideGrpinfoBlock = true;
                continue;
            }

            if (!isInsideGrpinfoBlock)
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
                        Size = size,
                    };

                    addons.Add(addon);
                }

                isInsideGrpinfoBlock = false;
            }
        }

        return addons;
    }

    private static string ExtractQuotedValue(ReadOnlySpan<char> span)
    {
        var pFrom = span.IndexOf('"') + 1;
        var pTo = span.LastIndexOf('"');

        if (pFrom > 0 && pTo > pFrom)
        {
            return span[pFrom..pTo].ToString();
        }

        return null!;
    }
}

public readonly struct GrpInfoEntry
{
    public readonly string Name { get; init; }
    public readonly string? MainCon { get; init; }
    public readonly string? AddDef { get; init; }
    public readonly int Size { get; init; }
}
