using Addons.Addons;
using Common;
using Common.Client.Config;
using Common.Enums;
using Common.Interfaces;
using Games.Games;
using Ports.Ports.EDuke32;

namespace Tests.CmdArguments;

[Collection("Sync")]
public sealed class FuryCmdArgumentsTests
{
    private readonly FuryGame _dukeGame;
    private readonly DukeCampaign _dukeCamp;

    private readonly AutoloadModsProvider _modsProvider;

    public FuryCmdArgumentsTests()
    {
        _modsProvider = new(GameEnum.Fury);

        _dukeGame = new()
        {
            GameInstallFolder = Path.Combine("D:", "Games", "Fury")
        };

        _dukeCamp = new()
        {
            Id = nameof(GameEnum.Duke3D).ToLower(),
            Type = AddonTypeEnum.Official,
            Title = "Ion Fury",
            GridImage = null,
            Author = null,
            Description = null,
            Version = null,
            SupportedGame = new(GameEnum.Fury),
            RequiredFeatures = null,
            PathToFile = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainCon = null,
            AdditionalCons = null,
            MainDef = null,
            RTS = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImage = null,
            IsFolder = false,
            Executables = null
        };
    }

    [Fact]
    public void FuryTest()
    {
        var mods = new List<AutoloadMod>() {
            _modsProvider.EnabledModWithCons,
            _modsProvider.DisabledMod,
            _modsProvider.ModThatRequiresOfficialAddon,
            _modsProvider.ModThatIncompatibleWithAddon,
            _modsProvider.IncompatibleMod,
            _modsProvider.IncompatibleModWithIncompatibleVersion,
            _modsProvider.IncompatibleModWithCompatibleVersion,
            _modsProvider.DependentMod,
            _modsProvider.DependentModWithCompatibleVersion,
            _modsProvider.DependentModWithIncompatibleVersion,
            _modsProvider.ModForAnotherGame,
            _modsProvider.ModThatRequiredFeature
        }.ToDictionary(x => new AddonVersion(x.Id, x.Version), x => (IAddon)x);

        Fury fury = new(new ConfigProviderFake());

        var args = fury.GetStartGameArgs(_dukeGame, _dukeCamp, mods, true, true, 3);
        var expected = @$" -quick -nosetup -g ""enabled_mod.zip"" -mh ""ENABLED1.DEF"" -mh ""ENABLED2.DEF"" -mx ""ENABLED1.CON"" -mx ""ENABLED2.CON"" -g ""mod_incompatible_with_addon.zip"" -g ""incompatible_mod_with_compatible_version.zip"" -g ""dependent_mod.zip"" -g ""dependent_mod_with_compatible_version.zip"" -g ""feature_mod.zip"" -j ""{Directory.GetCurrentDirectory()}\Data\Addons\Fury\Mods"" -s3";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }
}