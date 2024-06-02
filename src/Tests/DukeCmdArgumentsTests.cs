using Common;
using Common.Enums;
using Common.Enums.Addons;
using Common.Enums.Versions;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests;

[Collection("Sync")]
public class DukeCmdArgumentsTests
{
    private readonly DukeGame _dukeGame;
    private readonly DukeCampaign _dukeCamp;
    private readonly DukeCampaign _dukeVaca;
    private readonly DukeCampaign _dukeTC;
    private readonly DukeCampaign _dukeWtCamp;
    private readonly DukeCampaign _duke64Camp;

    private readonly AutoloadMod _enabledMod;
    private readonly AutoloadMod _modThatRequiresVaca;
    private readonly AutoloadMod _modThatIncompatibleWithVaca;
    private readonly AutoloadMod _incompatibleMod;
    private readonly AutoloadMod _incompatibleModWithIncompatibleVersion;
    private readonly AutoloadMod _incompatibleModWithCompatibleVersion;
    private readonly AutoloadMod _dependantMod;
    private readonly AutoloadMod _dependantModWithIncompatibleVersion;
    private readonly AutoloadMod _dependantModWithCompatibleVersion;
    private readonly AutoloadMod _disabledMod;
    private readonly AutoloadMod _eduke32mod;

    public DukeCmdArgumentsTests()
    {
        _dukeGame = new()
        {
            Duke64RomPath = @"D:\Games\Duke64\rom.z64",
            DukeWTInstallPath = @"D:\Games\DukeWT",
            GameInstallFolder = @"D:\Games\Duke3D"
        };

        _dukeCamp = new()
        {
            Id = nameof(GameEnum.Duke3D).ToLower(),
            Type = AddonTypeEnum.Official,
            Title = "Duke Nukem 3D",
            GridImage = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            PathToFile = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImage = null
        };

        _dukeWtCamp = new()
        {
            Id = nameof(DukeVersionEnum.Duke3D_WT).ToLower(),
            Type = AddonTypeEnum.Official,
            Title = "Duke Nukem 3D World Tour",
            GridImage = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_WT),
            RequiredFeatures = null,
            PathToFile = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImage = null
        };

        _duke64Camp = new()
        {
            Id = nameof(GameEnum.Duke64).ToLower(),
            Type = AddonTypeEnum.Official,
            Title = "Duke Nukem 64",
            GridImage = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.Duke64),
            RequiredFeatures = null,
            PathToFile = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImage = null
        };

        _dukeTC = new()
        {
            Id = "duke-tc",
            Type = AddonTypeEnum.Map,
            Title = "Duke Nukem 3D TC",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.1",
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            PathToFile = null,
            DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { nameof(DukeAddonEnum.DukeVaca), null } },
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImage = null
        };

        _dukeVaca = new()
        {
            Id = "dukevaca",
            Type = AddonTypeEnum.Official,
            Title = "Duke Nukem 3D Caribbean",
            GridImage = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            PathToFile = null,
            DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { nameof(DukeAddonEnum.DukeVaca), null } },
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            RTS = null,
            StartMap = null,
            PreviewImage = null
        };

        _enabledMod = new()
        {
            Id = "duke3d-enabledMod",
            Type = AddonTypeEnum.Mod,
            Title = "enabledMod",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.5",
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            IncompatibleAddons = null,
            RequiredFeatures = null,
            PathToFile = "enabled_mod.zip",
            DependentAddons = null,
            AdditionalCons = ["ENABLED1.CON", "ENABLED2.CON"],
            MainDef = null,
            AdditionalDefs = ["ENABLED1.DEF", "ENABLED2.DEF"],
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _modThatRequiresVaca = new()
        {
            Id = "duke3d-modThatRequiredVaca",
            Type = AddonTypeEnum.Mod,
            Title = "modThatRequiredVaca",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.5",
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            IncompatibleAddons = null,
            RequiredFeatures = null,
            PathToFile = "mod_requires_vaca.zip",
            DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { nameof(DukeAddonEnum.DukeVaca), null } },
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _modThatIncompatibleWithVaca = new()
        {
            Id = "duke3d-modThatIncompatibleWithVaca",
            Type = AddonTypeEnum.Mod,
            Title = "modThatIncompatibleWithVaca",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.5",
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            IncompatibleAddons = new(StringComparer.OrdinalIgnoreCase) { { nameof(DukeAddonEnum.DukeVaca), null } },
            DependentAddons = null,
            RequiredFeatures = null,
            PathToFile = "mod_incompatible_vaca.zip",
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _incompatibleMod = new()
        {
            Id = "duke3d-incompatibleMod",
            Type = AddonTypeEnum.Mod,
            Title = "incompatibleMod",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            PathToFile = "incompatible_mod.zip",
            DependentAddons = null,
            IncompatibleAddons = new(StringComparer.OrdinalIgnoreCase) { { "duke3d-enabledMod", null } },
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _incompatibleModWithCompatibleVersion = new()
        {
            Id = "duke3d-incompatibleModWithIncompatibleVersion",
            Type = AddonTypeEnum.Mod,
            Title = "incompatibleModWithIncompatibleVersion",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            PathToFile = "incompatible_mod_with_compatible_version.zip",
            DependentAddons = null,
            IncompatibleAddons = new(StringComparer.OrdinalIgnoreCase) { { "duke3d-enabledMod", "<=1.0" } },
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _incompatibleModWithIncompatibleVersion = new()
        {
            Id = "duke3d-incompatibleModWithCompatibleVersion",
            Type = AddonTypeEnum.Mod,
            Title = "incompatibleModWithCompatibleVersion",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            PathToFile = "incompatible_mod_with_incompatible_version.zip",
            DependentAddons = null,
            IncompatibleAddons = new(StringComparer.OrdinalIgnoreCase) { { "duke3d-enabledMod", ">1.1" } },
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _dependantMod = new()
        {
            Id = "duke3d-dependantMod",
            Type = AddonTypeEnum.Mod,
            Title = "dependantMod",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            PathToFile = "dependant_mod.zip",
            DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { "duke3d-enabledMod", null } },
            IncompatibleAddons = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _dependantModWithIncompatibleVersion = new()
        {
            Id = "duke3d-dependantModWithIncompatibleVersion",
            Type = AddonTypeEnum.Mod,
            Title = "dependantModWithIncompatibleVersion",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            PathToFile = "dependant_mod_with_incompatible_version.zip",
            DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { "duke3d-enabledMod", "<=1.0" } },
            IncompatibleAddons = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _dependantModWithCompatibleVersion = new()
        {
            Id = "duke3d-dependantModWithCompatibleVersion",
            Type = AddonTypeEnum.Mod,
            Title = "dependantModWithCompatibleVersion",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            PathToFile = "dependant_mod_with_compatible_version.zip",
            DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { "duke3d-enabledMod", ">1.1" } },
            IncompatibleAddons = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };

        _disabledMod = new()
        {
            Id = "duke3d-disabledMod",
            Type = AddonTypeEnum.Mod,
            Title = "disabledMod",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = null,
            PathToFile = "disabled_mod.zip",
            DependentAddons = null,
            IncompatibleAddons = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = false
        };

        _eduke32mod = new()
        {
            Id = "duke3d-eduke32mod",
            Type = AddonTypeEnum.Mod,
            Title = "eduke32mod",
            GridImage = null,
            Author = null,
            Description = null,
            Version = "1.0",
            SupportedGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            RequiredFeatures = [FeatureEnum.EDuke32_CON],
            PathToFile = "eduke32_mod.zip",
            DependentAddons = null,
            IncompatibleAddons = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsEnabled = true
        };
    }

    [Fact]
    public void RazeTest()
    {
        var mods = new List<AutoloadMod>() { 
            _enabledMod,
            _modThatRequiresVaca,
            _incompatibleMod,
            _disabledMod, 
            _incompatibleModWithIncompatibleVersion,
            _incompatibleModWithCompatibleVersion,
            _dependantMod,
            _dependantModWithIncompatibleVersion,
            _dependantModWithCompatibleVersion,
            _eduke32mod
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        Raze raze = new();

        var args = raze.GetStartGameArgs(_dukeGame, _dukeCamp, mods, true, true);
        var expected = @$" -quick -nosetup -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -addcon ""ENABLED1.CON"" -addcon ""ENABLED2.CON"" -file ""incompatible_mod_with_compatible_version.zip"" -file ""dependant_mod.zip"" -file ""dependant_mod_with_compatible_version.zip"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Ports\Raze\Save\duke3d"" -def ""a"" -addon 0";
   
        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', '/');
            expected = expected.Replace('\\', '/');
        }

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
            [GameSearch.Directories]
            Path=D:/Games/Duke3D

            [FileSearch.Directories]
            Path=D:/Games/Duke3D
            Path={Directory.GetCurrentDirectory()}/Data/Duke3D/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void RazeWtTest()
    {
        Raze raze = new();

        var args = raze.GetStartGameArgs(_dukeGame, _dukeWtCamp, [], true, true);
        var expected = @$" -quick -nosetup -savedir ""{Directory.GetCurrentDirectory()}\Data\Ports\Raze\Save\duke3d_wt"" -def ""a"" -addon 0";
   
        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', '/');
            expected = expected.Replace('\\', '/');
        }

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
            [GameSearch.Directories]
            Path=D:/Games/DukeWT

            [FileSearch.Directories]
            Path=D:/Games/DukeWT
            Path={Directory.GetCurrentDirectory()}/Data/Duke3D/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void RazeVacaTest()
    {
        var mods = new List<AutoloadMod>() { 
            _enabledMod,
            _modThatRequiresVaca,
            _modThatIncompatibleWithVaca
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        Raze raze = new();

        var args = raze.GetStartGameArgs(_dukeGame, _dukeVaca, mods, true, true);
        var expected = @$" -quick -nosetup -file ""enabled_mod.zip"" -adddef ""ENABLED1.DEF"" -adddef ""ENABLED2.DEF"" -addcon ""ENABLED1.CON"" -addcon ""ENABLED2.CON"" -file ""mod_requires_vaca.zip"" -savedir ""{Directory.GetCurrentDirectory()}\Data\Ports\Raze\Save\dukevaca"" -def ""a"" -addon 3";
   
        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', '/');
            expected = expected.Replace('\\', '/');
        }

        Assert.Equal(expected, args);

        var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ports", "Raze", "raze_portable.ini"));

        Assert.StartsWith($"""
            [GameSearch.Directories]
            Path=D:/Games/Duke3D

            [FileSearch.Directories]
            Path=D:/Games/Duke3D
            Path={Directory.GetCurrentDirectory()}/Data/Duke3D/Mods

            [SoundfontSearch.Directories]
            """.Replace('\\', '/'), config);
    }

    [Fact]
    public void EDuke32Test()
    {
        var mods = new List<AutoloadMod>() { 
            _enabledMod, 
            _modThatRequiresVaca,
            _incompatibleMod,
            _disabledMod, 
            _incompatibleModWithIncompatibleVersion,
            _incompatibleModWithCompatibleVersion,
            _dependantMod,
            _dependantModWithIncompatibleVersion,
            _dependantModWithCompatibleVersion,
            _eduke32mod
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        EDuke32 eduke32 = new();

        var args = eduke32.GetStartGameArgs(_dukeGame, _dukeCamp, mods, true, true, 3);
        var expected = @$" -quick -nosetup -j ""{Directory.GetCurrentDirectory()}\Data\Duke3D\Mods"" -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -mx ""ENABLED1.CON"" -mx ""ENABLED2.CON"" -g ""incompatible_mod_with_compatible_version.zip"" -g ""dependant_mod.zip"" -g ""dependant_mod_with_compatible_version.zip"" -g ""eduke32_mod.zip"" -usecwd -h ""a"" -j ""D:\Games\Duke3D"" -addon 0 -s3";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', '/');
            expected = expected.Replace('\\', '/');
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void Eduke32WtTest()
    {
        EDuke32 eduke32 = new();

        var args = eduke32.GetStartGameArgs(_dukeGame, _dukeWtCamp, [], true, true);
        var expected = @$" -quick -nosetup -usecwd -h ""a"" -j ""D:\Games\DukeWT"" -addon 0 -j ""{Directory.GetCurrentDirectory()}\Data\Duke3D\Special\WTStopgap"" -gamegrp e32wt.grp";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', '/');
            expected = expected.Replace('\\', '/');
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void Eduke32VacaTest()
    {
        var mods = new List<AutoloadMod>() {
            _enabledMod,
            _modThatRequiresVaca,
            _modThatIncompatibleWithVaca
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        EDuke32 eduke32 = new();

        var args = eduke32.GetStartGameArgs(_dukeGame, _dukeVaca, mods, true, true);
        var expected = @$" -quick -nosetup -j ""{Directory.GetCurrentDirectory()}\Data\Duke3D\Mods"" -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -mx ""ENABLED1.CON"" -mx ""ENABLED2.CON"" -g ""mod_requires_vaca.zip"" -usecwd -h ""a"" -j ""D:\Games\Duke3D"" -addon 3";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', '/');
            expected = expected.Replace('\\', '/');
        }

        Assert.Equal(expected, args);
    }

    [Fact]
    public void RedNukem64Test()
    {
        RedNukem redNukem = new();

        var args = redNukem.GetStartGameArgs(_dukeGame, _duke64Camp, [], true, true);
        var expected = @$" -quick -nosetup -usecwd -h ""a"" -j ""D:\Games\Duke64"" -gamegrp ""rom.z64""";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', '/');
            expected = expected.Replace('\\', '/');
        }

        Assert.Equal(expected, args);
    }
}