using Addons.Addons;
using Common.All.Enums;
using Common.All.Interfaces;
using Common.Client.Config;
using Games.Games;
using Ports.Ports.EDuke32;

namespace Tests.CmdArguments;

[Collection("Sync")]
public sealed class FuryCmdArgumentsTests
{
    private readonly FuryGame _dukeGame;
    private readonly DukeCampaignEntity _dukeCamp;

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
            AddonId = new(nameof(GameEnum.Fury).ToLower(), null),
            Type = AddonTypeEnum.Official,
            Title = "Ion Fury",
            GridImageHash = null,
            Author = null,
            Description = null,
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
            PreviewImageHash = null,
            IsFolder = false,
            Executables = null
        };
    }

    [Fact]
    public void FuryTest()
    {
        var mods = new List<AutoloadModEntity>() {
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
        }.ToDictionary(x => x.AddonId, x => (IAddon)x);

        Fury fury = new(new ConfigProviderFake());

        var args = fury.GetStartGameArgs(_dukeGame, _dukeCamp, mods, true, true, 3);
        var expected = $"" +
            $" -g \"enabled_mod.zip\"" +
            $" -mh \"ENABLED1.DEF\"" +
            $" -mh \"ENABLED2.DEF\"" +
            $" -mx \"ENABLED1.CON\"" +
            $" -mx \"ENABLED2.CON\"" +
            $" -g \"mod_incompatible_with_addon.zip\"" +
            $" -g \"incompatible_mod_with_compatible_version.zip\"" +
            $" -g \"dependent_mod.zip\"" +
            $" -g \"dependent_mod_with_compatible_version.zip\"" +
            $" -g \"feature_mod.zip\"" +
            $" -j \"{Directory.GetCurrentDirectory()}\\Data\\Addons\\Fury\\Mods\"" +
            $" -s3" +
            $" -quick" +
            $" -nosetup" +
            $"";

        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }

        Assert.Equal(expected, args);
    }
}