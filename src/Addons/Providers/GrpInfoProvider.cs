using Addons.Addons;
using Common;
using Common.Enums;
using Common.Enums.Versions;
using Common.Interfaces;
using CommunityToolkit.Diagnostics;

namespace Addons.Providers;

public static class GrpInfoProvider
{
    /// <summary>
    /// Get list of addons from grpinfo file
    /// </summary>
    public static List<IAddon>? GetAddonsFromGrpInfo(string campaignsFolder)
    {
        List<IAddon> newAddons = [];

        var grpInfos = Directory.GetFiles(campaignsFolder, "*.grpinfo", SearchOption.AllDirectories);

        if (grpInfos.Length == 0)
        {
            return null;
        }

        foreach (var grpInfo in grpInfos)
        {
            var grpInfoFolder = Path.GetDirectoryName(grpInfo) ?? ThrowHelper.ThrowInvalidOperationException<string>();

            var grps = Directory.GetFiles(grpInfoFolder, "*.grp", SearchOption.TopDirectoryOnly);

            if (grps.Length == 0)
            {
                continue;
            }

            var fileContent = File.ReadAllLines(grpInfo);
            var addons = Parse(fileContent, grps.Length);

            foreach (var grp in grps)
            {
                var fileSize = new FileInfo(grp).Length;

                var addon = addons.FirstOrDefault(x => x.Size == fileSize);

                if (addon.Equals(default(GrpInfo)) || addon.Name is null)
                {
                    continue;
                }

                AddonId version = new(addon.Name.ToLower().Replace(" ", "_"), null);

                DukeCampaignEntity camp = new()
                {
                    AddonId = version,
                    Type = AddonTypeEnum.TC,
                    SupportedGame = new(GameEnum.Duke3D, addon.DukeVersion),
                    Title = addon.Name,
                    GridImageHash = null,
                    PreviewImageHash = null,
                    Description = null,
                    Author = null,
                    PathToFile = grp,
                    DependentAddons = null,
                    IncompatibleAddons = null,
                    StartMap = null,
                    MainCon = addon.MainCon,
                    AdditionalCons = null,
                    MainDef = null,
                    AdditionalDefs = addon.AddDef is null ? null : [addon.AddDef],
                    RTS = null,
                    RequiredFeatures = [FeatureEnum.EDuke32_CON],
                    IsFolder = false,
                    Executables = null,
                    IsFavorite = false
                };

                newAddons.Add(camp);
            }
        }

        return newAddons;
    }

    /// <summary>
    /// Parse grpinfo file
    /// </summary>
    /// <param name="fileContent">Content of the grpinfo file</param>
    /// <param name="grpsCount">Number of found grps</param>
    private static List<GrpInfo> Parse(string[] fileContent, int grpsCount)
    {
        List<GrpInfo> addons = new(grpsCount);

        for (var i = 0; i < fileContent.Length; i++)
        {
            if (!fileContent[i].Trim().StartsWith("grpinfo"))
            {
                continue;
            }

            i++;
            var started = true;

            string name = null!;
            string mainCon = null!;
            string def = null!;
            var size = 0;
            DukeVersionEnum? dukeVersion = null;

            while (started)
            {
                try
                {
                    var str = fileContent[i];

                    if (str.Trim().StartsWith('}'))
                    {
                        started = false;
                        continue;
                    }
                    else if (str.Trim().StartsWith("name"))
                    {
                        var pFrom = str.IndexOf('"') + 1;
                        var pTo = str.LastIndexOf('"');

                        name = str[pFrom..pTo];
                    }
                    else if (str.Trim().StartsWith("scriptname"))
                    {
                        var pFrom = str.IndexOf('"') + 1;
                        var pTo = str.LastIndexOf('"');

                        mainCon = str[pFrom..pTo];
                    }
                    else if (str.Trim().StartsWith("defname"))
                    {
                        var pFrom = str.IndexOf('"') + 1;
                        var pTo = str.LastIndexOf('"');

                        def = str[pFrom..pTo];
                    }
                    else if (str.Trim().StartsWith("size"))
                    {
                        var result = str.Replace("size", "");

                        size = int.Parse(result);
                    }
                    else if (str.Trim().StartsWith("dependency"))
                    {
                        var result = str.Replace("dependency", "").Trim();

                        if (result.Equals("DUKE15_CRC"))
                        {
                            dukeVersion = DukeVersionEnum.Duke3D_Atomic;
                        }
                        else
                        {
                            dukeVersion = null;
                        }
                    }

                    i++;
                }
                catch
                {
                    continue;
                }
            }

            if (name is not null)
            {
                GrpInfo addon = new()
                {
                    Name = name,
                    MainCon = mainCon,
                    AddDef = def,
                    Size = size,
                    DukeVersion = dukeVersion
                };

                addons.Add(addon);
            }
        }

        return addons;
    }
}

public readonly struct GrpInfo
{
    public readonly string Name { get; init; }
    public readonly string MainCon { get; init; }
    public readonly string? AddDef { get; init; }
    public readonly int Size { get; init; }
    //public readonly int Crc { get; init; }
    public readonly DukeVersionEnum? DukeVersion { get; init; }
}
