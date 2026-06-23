using Core.All.Enums;
using Core.All.Serializable.Addon;

namespace Core.Client.Helpers;

public static class UriHelper
{
    public static string GetRelativeFilePath(AddonManifestJsonModel manifest, string pathToFile)
    {
        var folderName = manifest.AddonType switch
        {
            AddonTypeEnum.TC => "Campaigns",
            AddonTypeEnum.Map => "Maps",
            AddonTypeEnum.Mod => "Mods",
            _ => throw new NotSupportedException(),
        };

        var gameName = manifest.SupportedGame.Game switch
        {
            GameEnum.Duke3D => "Duke3D",
            GameEnum.Duke64 => "Duke64",
            GameEnum.Blood => "Blood",
            GameEnum.Wang => "Wang",
            GameEnum.Fury => "Fury",
            GameEnum.Slave => "Slave",
            GameEnum.NAM => "NAM",
            GameEnum.WW2GI => "WW2GI",
            GameEnum.Redneck or GameEnum.RidesAgain => "Redneck",
            GameEnum.TekWar => "TekWar",
            GameEnum.Witchaven => "WH",
            GameEnum.Witchaven2 => "WH2",
            GameEnum.Standalone => "Standalone",
            _ => throw new NotSupportedException(),
        };

        return $"{gameName}/{folderName}/{Path.GetFileName(pathToFile)}";
    }
}
