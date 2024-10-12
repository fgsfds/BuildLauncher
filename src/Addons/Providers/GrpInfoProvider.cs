using Addons.Addons;
using Common.Enums;
using Common.Enums.Versions;
using Common.Interfaces;

namespace Addons.Providers;

public static class GrpInfoProvider
{
    /// <summary>
    /// Get list of addons from grpinfo file
    /// </summary>
    /// <param name="foldersGrpInfos">Folders to search for grpinfo and grps</param>
    public static List<IAddon> GetAddonsFromGrpInfo(List<string> foldersGrpInfos)
    {
        List<IAddon> newAddons = [];

        foreach (var folder in foldersGrpInfos)
        {
            var grpInfo = Path.Combine(folder, "addons.grpinfo");
            var grps = Directory.GetFiles(folder, "*.grp");

            if (grps.Length == 0)
            {
                continue;
            }

            var addons = Parse(grpInfo, grps.Length);

            foreach (var grp in grps)
            {
                var fileSize = new FileInfo(grp).Length;

                var addon = addons.Find(x => x.Size == fileSize);

                if (addon.Name is null)
                {
                    continue;
                }

                DukeCampaign camp = new()
                {
                    Id = addon.Name.ToLower().Replace(" ", "_"),
                    Type = AddonTypeEnum.TC,
                    SupportedGame = new(GameEnum.Duke3D, addon.DukeVersion),
                    Title = addon.Name,
                    GridImage = null,
                    Description = null,
                    Version = null,
                    Author = null,
                    PathToFile = grp,
                    DependentAddons = null,
                    IncompatibleAddons = null,
                    StartMap = null,
                    MainCon = addon.MainCon,
                    AdditionalCons = null,
                    MainDef = null,
                    AdditionalDefs = [addon.AddDef],
                    RTS = null,
                    RequiredFeatures = [FeatureEnum.EDuke32_CON],
                    PreviewImage = null,
                    IsFolder = false,
                    Executables = null
                };

                newAddons.Add(camp);
            }
        }

        return newAddons;
    }

    /// <summary>
    /// Parse grpinfo file
    /// </summary>
    /// <param name="pathToFile">Path to grpinfo file</param>
    /// <param name="grpsCount">Number of found grps</param>
    private static List<GrpInfo> Parse(string pathToFile, int grpsCount)
    {
        var file = File.ReadAllLines(pathToFile);

        List<GrpInfo> addons = new(grpsCount);

        for (var i = 0; i < file.Length; i++)
        {
            if (!file[i].Trim().StartsWith("grpinfo"))
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
                    var str = file[i];

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
