using System.Collections.Immutable;
using Addons.Providers;
using Core.All;
using Core.All.Enums;
using Core.All.Serializable.Addon;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Games.Games;

namespace Addons.Addons;

/// <summary>
///     Builds <see cref="BaseAddon" /> objects from parsed addon files and manifests.
/// </summary>
internal sealed class AddonFactory
{
    private readonly IConfigProvider _config;
    private readonly GameEnum _gameEnum;
    private readonly MetadataProvider _metadataProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonFactory" /> class.
    /// </summary>
    /// <param name="game">The game to scope addon construction to.</param>
    /// <param name="config">Configuration provider.</param>
    /// <param name="metadataProvider">Metadata provider.</param>
    public AddonFactory(BaseGame game, IConfigProvider config, MetadataProvider metadataProvider)
    {
        _gameEnum = game.GameEnum;
        _config = config;
        _metadataProvider = metadataProvider;
    }

    /// <summary>
    ///     Convert a parsed addon file with a manifest into <see cref="BaseAddon" />.
    /// </summary>
    /// <param name="parsedAddonFile">Parsed addon file with a non-null manifest.</param>
    /// <returns>Addon, or <see langword="null" /> if the file type is not supported.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="parsedAddonFile" /> has a null manifest.</exception>
    /// <exception cref="ArgumentException">An autoload mod manifest contains a MainDef value.</exception>
    internal BaseAddon? GetAddonFromFile(ParsedAddonFile parsedAddonFile)
    {
        if (parsedAddonFile.Manifest is null)
        {
            throw new InvalidOperationException($"{nameof(GetAddonFromFile)} requires a non-null manifest. File: {parsedAddonFile.FileInfo.PathToFile}");
        }

        AddonCarcass? carcass;
        BaseAddon? addon;

        if (parsedAddonFile.FileInfo.IsJson || parsedAddonFile.FileInfo.IsZip || parsedAddonFile.FileInfo.IsFolder)
        {
            carcass = GetCarcass(parsedAddonFile.Manifest, parsedAddonFile.FileInfo, parsedAddonFile.GridHash, parsedAddonFile.PreviewHash);
        }
        else if (parsedAddonFile.FileInfo.IsGrpInfo || parsedAddonFile.FileInfo.IsMap)
        {
            throw new NotSupportedException();
        }
        else
        {
            return null;
        }

        var carcassValue = carcass.Value;

        if (carcassValue.Type is AddonTypeEnum.Mod)
        {
            var isEnabled = !_config.DisabledAutoloadMods.Contains(carcassValue.Id);

            if (carcassValue.MainDef is not null)
            {
                throw new ArgumentException("Autoload mod can't have Main DEF");
            }

            AddonId id = new(carcassValue.Id, carcassValue.Version);

            addon = new AutoloadMod
            {
                AddonId = id,
                Type = AddonTypeEnum.Mod,
                Title = carcassValue.Title,
                GridImageHash = carcassValue.GridImageHash,
                PreviewImageHash = carcassValue.PreviewImageHash,
                Description = carcassValue.Description,
                Author = carcassValue.Author,
                ReleaseDate = carcassValue.ReleaseDate,
                IsEnabled = isEnabled,
                FileInfo = parsedAddonFile.FileInfo,
                MainDef = null,
                AdditionalDefs = carcassValue.AddDefs,
                AdditionalCons = carcassValue.AddCons,
                SupportedGame = new(carcassValue.SupportedGame, carcassValue.GameVersion, carcassValue.GameCrc),
                DependentAddons = carcassValue.Dependencies,
                IncompatibleAddons = carcassValue.Incompatibles,
                StartMap = carcassValue.StartMap,
                RequiredFeatures = carcassValue.RequiredFeatures,
                Executables = null,
                Options = null,
                IsFavorite = _config.FavoriteAddons.Contains(id),
                IsMetadataUpdateAvailable = _metadataProvider.IsMetadataUpdateAvailable(id, parsedAddonFile.FileInfo)
            };
        }
        else
        {
            addon = CreateCampaignAddon(carcassValue, parsedAddonFile.FileInfo);
        }

        return addon;
    }

    /// <summary>
    ///     Create a <see cref="LooseMap" /> from a parsed .map file, looking up a matching .ini in the same folder.
    /// </summary>
    /// <param name="parsedAddonFile">Parsed addon file for a loose map.</param>
    public BaseAddon? GetLooseMapFromFile(ParsedAddonFile parsedAddonFile)
    {
        if (!parsedAddonFile.FileInfo.IsMap)
        {
            return null;
        }

        var bloodIniName = parsedAddonFile.FileInfo.FileName.Replace(".map", ".ini", StringComparison.InvariantCultureIgnoreCase);
        var actualIni = Path.GetFileName(Directory.EnumerateFiles(parsedAddonFile.FileInfo.PathToFolder).FirstOrDefault(f => Path.GetFileName(f).Equals(bloodIniName, StringComparison.OrdinalIgnoreCase)));

        AddonId id = new(parsedAddonFile.FileInfo.FileName);

        return new LooseMap
        {
            AddonId = id,
            Type = AddonTypeEnum.Map,
            FileInfo = parsedAddonFile.FileInfo,
            Title = parsedAddonFile.FileInfo.FileName,
            SupportedGame = new(_gameEnum, null, null),
            StartMap = new MapFileJsonModel
            {
                File = parsedAddonFile.FileInfo.FileName
            },
            BloodIni = actualIni,
            GridImageHash = null,
            Description = null,
            Author = null,
            ReleaseDate = null,
            MainDef = null,
            AdditionalDefs = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            RequiredFeatures = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null,
            IsFavorite = _config.FavoriteAddons.Contains(id),
            IsMetadataUpdateAvailable = _metadataProvider.IsMetadataUpdateAvailable(id, parsedAddonFile.FileInfo)
        };
    }

    private BaseAddon CreateCampaignAddon(AddonCarcass carcass, AddonFilePathWrapper fileInfo)
    {
        AddonId id = new(carcass.Id, carcass.Version);
        var game = new GameInfo(carcass.SupportedGame, carcass.GameVersion, carcass.GameCrc);
        var isFavorite = _config.FavoriteAddons.Contains(id);
        var isUpdate = _metadataProvider.IsMetadataUpdateAvailable(id, fileInfo);

        return _gameEnum switch
        {
            GameEnum.Duke3D
             or GameEnum.Fury
             or GameEnum.Redneck
             or GameEnum.NAM
             or GameEnum.WW2GI =>
                new DukeCampaign
                {
                    AddonId = id,
                    Type = carcass.Type,
                    Title = carcass.Title,
                    GridImageHash = carcass.GridImageHash,
                    PreviewImageHash = carcass.PreviewImageHash,
                    Description = carcass.Description,
                    Author = carcass.Author,
                    ReleaseDate = carcass.ReleaseDate,
                    FileInfo = fileInfo,
                    DependentAddons = carcass.Dependencies,
                    IncompatibleAddons = carcass.Incompatibles,
                    StartMap = carcass.StartMap,
                    MainCon = carcass.MainCon,
                    AdditionalCons = carcass.AddCons,
                    MainDef = carcass.MainDef,
                    AdditionalDefs = carcass.AddDefs,
                    RTS = carcass.Rts,
                    RequiredFeatures = carcass.RequiredFeatures,
                    Executables = carcass.Executables,
                    Options = carcass.Options,
                    SupportedGame = game,
                    IsFavorite = isFavorite,
                    IsMetadataUpdateAvailable = isUpdate
                },
            GameEnum.Wang or GameEnum.Slave =>
                new GenericCampaign
                {
                    AddonId = id,
                    Type = carcass.Type,
                    Title = carcass.Title,
                    GridImageHash = carcass.GridImageHash,
                    PreviewImageHash = carcass.PreviewImageHash,
                    Description = carcass.Description,
                    Author = carcass.Author,
                    ReleaseDate = carcass.ReleaseDate,
                    FileInfo = fileInfo,
                    DependentAddons = carcass.Dependencies,
                    IncompatibleAddons = carcass.Incompatibles,
                    StartMap = carcass.StartMap,
                    MainDef = carcass.MainDef,
                    AdditionalDefs = carcass.AddDefs,
                    RequiredFeatures = carcass.RequiredFeatures,
                    Executables = carcass.Executables,
                    Options = carcass.Options,
                    SupportedGame = game,
                    IsFavorite = isFavorite,
                    IsMetadataUpdateAvailable = isUpdate
                },
            GameEnum.Blood =>
                new BloodCampaign
                {
                    AddonId = id,
                    Type = carcass.Type,
                    Title = carcass.Title,
                    GridImageHash = carcass.GridImageHash,
                    PreviewImageHash = carcass.PreviewImageHash,
                    Description = carcass.Description,
                    Author = carcass.Author,
                    ReleaseDate = carcass.ReleaseDate,
                    FileInfo = fileInfo,
                    DependentAddons = carcass.Dependencies,
                    IncompatibleAddons = carcass.Incompatibles,
                    StartMap = carcass.StartMap,
                    MainDef = carcass.MainDef,
                    AdditionalDefs = carcass.AddDefs,
                    INI = carcass.Ini,
                    RFF = carcass.Rff,
                    SND = carcass.Snd,
                    RequiredFeatures = carcass.RequiredFeatures,
                    Executables = carcass.Executables,
                    Options = carcass.Options,
                    SupportedGame = game,
                    IsFavorite = isFavorite,
                    IsMetadataUpdateAvailable = isUpdate
                },
            GameEnum.Standalone =>
                new StandaloneGame
                {
                    AddonId = id,
                    Type = carcass.Type,
                    Title = carcass.Title,
                    GridImageHash = carcass.GridImageHash,
                    PreviewImageHash = carcass.PreviewImageHash,
                    Description = carcass.Description,
                    Author = carcass.Author,
                    ReleaseDate = carcass.ReleaseDate,
                    FileInfo = fileInfo,
                    DependentAddons = carcass.Dependencies,
                    IncompatibleAddons = carcass.Incompatibles,
                    StartMap = carcass.StartMap,
                    MainDef = carcass.MainDef,
                    AdditionalDefs = carcass.AddDefs,
                    RequiredFeatures = carcass.RequiredFeatures,
                    Executables = carcass.Executables,
                    Options = carcass.Options,
                    SupportedGame = game,
                    IsFavorite = isFavorite,
                    IsMetadataUpdateAvailable = isUpdate
                },
            _ => throw new NotSupportedException()
        };
    }

    private static AddonCarcass GetCarcass(
        AddonManifestJsonModel manifest,
        AddonFilePathWrapper fileInfo,
        long? gridImageHash,
        long? previewImageHash)
    {
        AddonCarcass carcass = new()
        {
            GridImageHash = gridImageHash ?? previewImageHash,
            PreviewImageHash = previewImageHash,
            Type = manifest.AddonType,
            Id = manifest.Id,
            Title = manifest.Title,
            Author = manifest.Author,
            ReleaseDate = manifest.ReleaseDate,
            Version = manifest.Version,
            Description = manifest.Description,
            SupportedGame = manifest.SupportedGame.Game,
            GameVersion = manifest.SupportedGame.Version,
            GameCrc = manifest.SupportedGame.Crc,
            Rts = manifest.Rts,
            Ini = manifest.Ini,
            Rff = manifest.MainRff,
            Snd = manifest.SoundRff,
            StartMap = manifest.StartMap,
            RequiredFeatures = manifest.Dependencies?.RequiredFeatures?.ToImmutableArray(),
            MainCon = manifest.MainCon,
            AddCons = manifest.AdditionalCons?.ToImmutableArray(),
            MainDef = manifest.MainDef,
            AddDefs = manifest.AdditionalDefs?.ToImmutableArray(),
            Dependencies = manifest.Dependencies?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase),
            Incompatibles = manifest.Incompatibles?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase)
        };

        if (manifest.Executables is not null)
        {
            carcass.Executables = [];

            foreach (var osPortsPair in manifest.Executables)
            {
                carcass.Executables.Add(osPortsPair.Key, []);

                foreach (var x in osPortsPair.Value)
                {
                    carcass.Executables[osPortsPair.Key].Add(x.Key, Path.Combine(fileInfo.PathToFolder, x.Value));
                }
            }
        }
        else
        {
            carcass.Executables = null;
        }

        if (manifest.Options is not null)
        {
            carcass.Options = [];

            foreach (var option in manifest.Options)
            {
                carcass.Options.Add(option.OptionName, []);

                if (option.Parameters is not null)
                {
                    foreach (var param in option.Parameters)
                    {
                        carcass.Options[option.OptionName].Add(param.Key, param.Value);
                    }
                }
            }
        }
        else
        {
            carcass.Options = null;
        }

        return carcass;
    }
}
