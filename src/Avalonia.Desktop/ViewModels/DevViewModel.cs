using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Avalonia.Desktop.Helpers;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.All.Enums;
using Core.All.Enums.Versions;
using Core.All.Interfaces;
using Core.All.Serializable.Addon;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Core.Client.Tools;
using Games.Providers;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class DevViewModel : ObservableObject
{
    private readonly AddonsDatabaseManager _addonsDatabaseManager;

    private readonly IConfigProvider _config;

    private readonly IFilesUploader _filesUploader;

    /// <summary>
    ///     Set of forbidden file names that cannot be used in addons.
    /// </summary>
    private readonly HashSet<string> _forbiddenNames =
    [
        "duke3d.def",
        "rr.def",
        "rrra.def",
        "nam.def",
        "napalm.def",
        "ww2gi.def",
        "blood.def",
        "sw.def",
        "exhumed.def",
        "game.con",
        "defs.con",
        "user.con",
        "nam.con",
        "napalm.con",
        "ww2gi.con",
        "blood.ini",
        "cryptic.ini"
    ];

    /// <summary>
    ///     Set of forbidden OGG file names for Blood addons.
    /// </summary>
    private readonly HashSet<string> _forbiddenOggs =
    [
        "blood00.ogg",
        "blood01.ogg",
        "blood02.ogg",
        "blood03.ogg",
        "blood04.ogg",
        "blood05.ogg",
        "blood06.ogg",
        "blood07.ogg",
        "blood08.ogg",
        "blood09.ogg"
    ];

    private readonly InstalledGamesProvider _gamesProvider;

    private readonly ILogger<DevViewModel> _logger;


    /// <summary>
    ///     Initializes a new instance of the <see cref="DevViewModel" /> class.
    /// </summary>
    /// <param name="config">The configuration provider.</param>
    /// <param name="filesUploader">The files uploader.</param>
    /// <param name="addonsDatabaseManager">The addons database manager.</param>
    /// <param name="gamesProvider">The installed games provider.</param>
    /// <param name="logger">The logger.</param>
    public DevViewModel(
        IConfigProvider config,
        IFilesUploader filesUploader,
        AddonsDatabaseManager addonsDatabaseManager,
        InstalledGamesProvider gamesProvider,
        ILogger<DevViewModel> logger
        )
    {
        _config = config;
        _filesUploader = filesUploader;
        _addonsDatabaseManager = addonsDatabaseManager;
        _gamesProvider = gamesProvider;
        _logger = logger;

        ApiPasswordTextBox = _config.ApiPassword ?? string.Empty;
    }


    /// <summary>
    ///     Gets the addon manifest JSON model from the current form state.
    /// </summary>
    /// <param name="jsonString">The serialized JSON string.</param>
    /// <returns>The addon manifest.</returns>
    private AddonManifestJsonModel GetAddonJson(out string jsonString)
    {
        if (PathToAddonFolder is null)
        {
            throw new MissingFieldException("Choose addon folder");
        }

        var files = Directory.GetFiles(PathToAddonFolder, "*", SearchOption.TopDirectoryOnly)
                             .Select(static x => Path.GetFileName(x).ToLower())
                             .ToList();

        if (SelectedGame is not GameEnum.Standalone &&
            WindowsNBloodExe is null &&
            WindowsNotBloodExe is null &&
            WindowsEDukeExe is null &&
            WindowsRedNukemExe is null &&
            WindowsRazeExe is null &&
            WindowsPCExhumedExe is null &&
            LinuxNBloodExe is null &&
            LinuxNotBloodExe is null &&
            LinuxEDukeExe is null &&
            LinuxRedNukemExe is null &&
            LinuxRazeExe is null &&
            LinuxPCExhumedExe is null &&
            MainRff is null &&
            SoundRff is null)
        {
            var forbidden = files.Intersect(_forbiddenNames);

            if (forbidden.Any())
            {
                throw new MissingFieldException($"Common file names can't be used. Rename these files: {string.Join(", ", forbidden)}");
            }

            if (SelectedGame is GameEnum.Blood)
            {
                if (string.IsNullOrWhiteSpace(MainRff) && string.IsNullOrWhiteSpace(SoundRff))
                {
                    if (files.Any(static x => x.EndsWith(".ART", StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new MissingFieldException("Don't use ART files. Convert them to DEF.");
                    }

                    if (files.Any(static x => x.EndsWith(".DAT", StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new MissingFieldException("Don't use DAT files. Convert them to DEF.");
                    }

                    if (files.Any(static x => x.EndsWith(".RFS", StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new MissingFieldException("Addons with RFS files are not supported");
                    }
                }

                if (files.Intersect(_forbiddenOggs).Any())
                {
                    throw new MissingFieldException("blood00.ogg - blood09.ogg can't be used, start with blood10.ogg");
                }
            }
        }

        ErrorText = null;

        var addonType =
            IsTcSelected ? AddonTypeEnum.TC
            : IsMapSelected ? AddonTypeEnum.Map
            : IsModSelected ? AddonTypeEnum.Mod
            : throw new ArgumentOutOfRangeException("Select addon type");

        var gameEnum = SelectedGame ?? throw new ArgumentOutOfRangeException("Select game");

        DukeVersionEnum? dukeVersion =
            SelectedGame is not GameEnum.Duke3D ? null
            : IsDukeAtomicSelected ? DukeVersionEnum.Duke3D_Atomic
            : IsDuke13DSelected ? DukeVersionEnum.Duke3D_13D
            : IsDukeWTSelected ? DukeVersionEnum.Duke3D_WT
            : null;

        if (string.IsNullOrWhiteSpace(AddonTitle))
        {
            throw new MissingFieldException("Select addon title");
        }

        if (string.IsNullOrWhiteSpace(AddonId))
        {
            throw new MissingFieldException("Select addon id");
        }

        if (string.IsNullOrWhiteSpace(AddonVersion))
        {
            throw new MissingFieldException("Select addon version");
        }

        List<FeatureEnum> features = [];

        if (IsEdukeConsSelected &&
            AreDukePropertiesAvailable)
        {
            features.Add(FeatureEnum.EDuke32_CON);
        }

        if (IsModernTypesSelected &&
            SelectedGame is GameEnum.Blood)
        {
            features.Add(FeatureEnum.Modern_Types);
        }

        if (IsModelsSelected)
        {
            features.Add(FeatureEnum.Models);
        }

        if (IsHightileSelected)
        {
            features.Add(FeatureEnum.Hightile);
        }

        if (IsSlopedSelected)
        {
            features.Add(FeatureEnum.Sloped_Sprites);
        }

        if (IsTrorSelected)
        {
            features.Add(FeatureEnum.TROR);
        }

        if (IsCstatSelected)
        {
            features.Add(FeatureEnum.Wall_Rotate_Cstat);
        }

        if (IsLightingSelected)
        {
            features.Add(FeatureEnum.Dynamic_Lighting);
        }

        if (IsSndInfoSelected)
        {
            features.Add(FeatureEnum.SndInfo);
        }

        if (IsTilefromtextureSelected)
        {
            features.Add(FeatureEnum.TileFromTexture);
        }

        IStartMap? startMap = null;

        if (IsMapSelected)
        {
            if (!IsElMapTypeSelected && !IsFileMapTypeSelected)
            {
                throw new MissingFieldException("Select start map");
            }

            if (IsElMapTypeSelected)
            {
                if (MapEpisode is null)
                {
                    throw new MissingFieldException("Select start map episode");
                }

                if (MapLevel is null)
                {
                    throw new MissingFieldException("Select start map level");
                }

                startMap = new MapSlotJsonModel()
                {
                    Episode = MapEpisode.Value,
                    Level = MapLevel.Value
                };
            }

            if (IsFileMapTypeSelected)
            {
                if (string.IsNullOrWhiteSpace(MapFileName))
                {
                    throw new MissingFieldException("Select start map file name");
                }

                startMap = new MapFileJsonModel()
                {
                    File = MapFileName
                };
            }
        }

        Dictionary<OSEnum, Dictionary<PortEnum, string>>? executables = [];

        if (!string.IsNullOrWhiteSpace(WindowsNBloodExe))
        {
            _ = executables.TryAdd(OSEnum.Windows, []);
            executables[OSEnum.Windows].Add(PortEnum.NBlood, WindowsNBloodExe);
        }

        if (!string.IsNullOrWhiteSpace(WindowsNotBloodExe))
        {
            _ = executables.TryAdd(OSEnum.Windows, []);
            executables[OSEnum.Windows].Add(PortEnum.NotBlood, WindowsNotBloodExe);
        }

        if (!string.IsNullOrWhiteSpace(WindowsEDukeExe))
        {
            _ = executables.TryAdd(OSEnum.Windows, []);
            executables[OSEnum.Windows].Add(PortEnum.EDuke32, WindowsEDukeExe);
        }

        if (!string.IsNullOrWhiteSpace(WindowsRedNukemExe))
        {
            _ = executables.TryAdd(OSEnum.Windows, []);
            executables[OSEnum.Windows].Add(PortEnum.RedNukem, WindowsRedNukemExe);
        }

        if (!string.IsNullOrWhiteSpace(WindowsRazeExe))
        {
            _ = executables.TryAdd(OSEnum.Windows, []);
            executables[OSEnum.Windows].Add(PortEnum.Raze, WindowsRazeExe);
        }

        if (!string.IsNullOrWhiteSpace(WindowsPCExhumedExe))
        {
            _ = executables.TryAdd(OSEnum.Windows, []);
            executables[OSEnum.Windows].Add(PortEnum.PCExhumed, WindowsPCExhumedExe);
        }

        if (!string.IsNullOrWhiteSpace(LinuxNBloodExe))
        {
            _ = executables.TryAdd(OSEnum.Linux, []);
            executables[OSEnum.Linux].Add(PortEnum.NBlood, LinuxNBloodExe);
        }

        if (!string.IsNullOrWhiteSpace(LinuxNotBloodExe))
        {
            _ = executables.TryAdd(OSEnum.Linux, []);
            executables[OSEnum.Linux].Add(PortEnum.NotBlood, LinuxNotBloodExe);
        }

        if (!string.IsNullOrWhiteSpace(LinuxEDukeExe))
        {
            _ = executables.TryAdd(OSEnum.Linux, []);
            executables[OSEnum.Linux].Add(PortEnum.EDuke32, LinuxEDukeExe);
        }

        if (!string.IsNullOrWhiteSpace(LinuxRedNukemExe))
        {
            _ = executables.TryAdd(OSEnum.Linux, []);
            executables[OSEnum.Linux].Add(PortEnum.RedNukem, LinuxRedNukemExe);
        }

        if (!string.IsNullOrWhiteSpace(LinuxRazeExe))
        {
            _ = executables.TryAdd(OSEnum.Linux, []);
            executables[OSEnum.Linux].Add(PortEnum.Raze, LinuxRazeExe);
        }

        if (!string.IsNullOrWhiteSpace(LinuxPCExhumedExe))
        {
            _ = executables.TryAdd(OSEnum.Linux, []);
            executables[OSEnum.Linux].Add(PortEnum.PCExhumed, LinuxPCExhumedExe);
        }

        AddonManifestJsonModel addon = new()
        {
            AddonType = addonType,
            Id = AddonIdPrefix + AddonId,
            SupportedGame = new()
            {
                Game = gameEnum,
                Version = dukeVersion?.ToString(),
                Crc = string.IsNullOrWhiteSpace(GameCrc) ? null : GameCrc
            },
            Title = AddonTitle,
            Version = AddonVersion,
            ReleaseDate = ReleaseDate.HasValue ? DateOnly.FromDateTime(ReleaseDate.Value.DateTime) : null,
            Author = string.IsNullOrWhiteSpace(AddonAuthor) ? null : AddonAuthor,
            Description = string.IsNullOrWhiteSpace(AddonDescription) ? null : AddonDescription,
            MainDef = string.IsNullOrWhiteSpace(MainDef) || IsModSelected ? null : MainDef,
            AdditionalDefs = string.IsNullOrWhiteSpace(AdditionalDefs) ? null : [.. AdditionalDefs.Split(',')],
            MainCon = string.IsNullOrWhiteSpace(MainCon) || !IsMainConAvailable ? null : MainCon,
            AdditionalCons = string.IsNullOrWhiteSpace(AdditionalCons) || !AreDukePropertiesAvailable ? null : [.. AdditionalCons.Split(',')],
            Rts = string.IsNullOrWhiteSpace(Rts) ? null : Rts,
            Ini = string.IsNullOrWhiteSpace(Ini) ? null : Ini,
            MainRff = string.IsNullOrWhiteSpace(MainRff) ? null : MainRff,
            SoundRff = string.IsNullOrWhiteSpace(SoundRff) ? null : SoundRff,
            Dependencies = (DependenciesList is null || DependenciesList.Count == 0) && features.Count == 0
                ? null
                : new()
                {
                    Addons = DependenciesList is null || DependenciesList.Count == 0 ? null : [.. DependenciesList],
                    RequiredFeatures = features.Count == 0 ? null : features
                },
            Incompatibles = (IncompatibilitiesList is null || IncompatibilitiesList.Count == 0)
                ? null
                : new()
                {
                    Addons = [.. IncompatibilitiesList]
                },
            Options = (OptionsList is null || OptionsList.Count == 0) ? null : [.. OptionsList],
            StartMap = startMap,
            Executables = executables.Count == 0 ? null : executables
        };

        jsonString = JsonSerializer.Serialize(addon, AddonManifestJsonContext.Default.AddonManifestJsonModel);
        JsonText = jsonString;

        return addon;
    }

    /// <summary>
    ///     Loads an addon manifest from a JSON file.
    /// </summary>
    /// <param name="pathToFile">The path to the JSON file.</param>
    private void LoadJson(string pathToFile)
    {
        using var jsonStream = File.OpenRead(pathToFile);

        var addon = JsonSerializer.Deserialize(
            jsonStream,
            AddonManifestJsonContext.Default.AddonManifestJsonModel
            );

        if (addon is null)
        {
            return;
        }

        IsTcSelected = addon.AddonType is AddonTypeEnum.TC;
        IsMapSelected = addon.AddonType is AddonTypeEnum.Map;
        IsModSelected = addon.AddonType is AddonTypeEnum.Mod;

        SelectedGame = addon.SupportedGame.Game;

        var isDukeVersion = Enum.TryParse<DukeVersionEnum>(addon.SupportedGame.Version, true, out var dukeVersion);
        IsDuke13DSelected = isDukeVersion && dukeVersion is DukeVersionEnum.Duke3D_13D;
        IsDukeAtomicSelected = isDukeVersion && dukeVersion is DukeVersionEnum.Duke3D_Atomic;
        IsDukeWTSelected = isDukeVersion && dukeVersion is DukeVersionEnum.Duke3D_WT;

        GameCrc = addon.SupportedGame.Crc;

        AddonTitle = addon.Title;
        AddonId = string.IsNullOrEmpty(AddonIdPrefix) ? addon.Id : addon.Id.Replace(AddonIdPrefix, "");
        AddonVersion = addon.Version;
        ReleaseDate = addon.ReleaseDate.HasValue ? addon.ReleaseDate.Value.ToDateTime(TimeOnly.MinValue) : null;
        AddonAuthor = addon.Author;
        MainDef = addon.MainDef;
        AdditionalDefs = addon.AdditionalDefs is null ? null : string.Join(',', addon.AdditionalDefs);
        MainCon = addon.MainCon;
        AdditionalCons = addon.AdditionalCons is null ? null : string.Join(',', addon.AdditionalCons);
        Rts = addon.Rts;
        Ini = addon.Ini;
        MainRff = addon.MainRff;
        SoundRff = addon.SoundRff;

        IsEdukeConsSelected = addon.Dependencies?.RequiredFeatures is not null && addon.Dependencies.RequiredFeatures.Contains(FeatureEnum.EDuke32_CON);
        IsModernTypesSelected = addon.Dependencies?.RequiredFeatures is not null && addon.Dependencies.RequiredFeatures.Contains(FeatureEnum.Modern_Types);
        IsModelsSelected = addon.Dependencies?.RequiredFeatures is not null && addon.Dependencies.RequiredFeatures.Contains(FeatureEnum.Models);
        IsHightileSelected = addon.Dependencies?.RequiredFeatures is not null && addon.Dependencies.RequiredFeatures.Contains(FeatureEnum.Hightile);
        IsSlopedSelected = addon.Dependencies?.RequiredFeatures is not null && addon.Dependencies.RequiredFeatures.Contains(FeatureEnum.Sloped_Sprites);
        IsTrorSelected = addon.Dependencies?.RequiredFeatures is not null && addon.Dependencies.RequiredFeatures.Contains(FeatureEnum.TROR);
        IsCstatSelected = addon.Dependencies?.RequiredFeatures is not null && addon.Dependencies.RequiredFeatures.Contains(FeatureEnum.Wall_Rotate_Cstat);
        IsLightingSelected = addon.Dependencies?.RequiredFeatures is not null && addon.Dependencies.RequiredFeatures.Contains(FeatureEnum.Dynamic_Lighting);
        IsSndInfoSelected = addon.Dependencies?.RequiredFeatures is not null && addon.Dependencies.RequiredFeatures.Contains(FeatureEnum.SndInfo);
        IsTilefromtextureSelected = addon.Dependencies?.RequiredFeatures is not null && addon.Dependencies.RequiredFeatures.Contains(FeatureEnum.TileFromTexture);

        DependenciesList = addon.Dependencies?.Addons is null ? null : [.. addon.Dependencies.Addons];
        IncompatibilitiesList = addon.Incompatibles?.Addons is null ? null : [.. addon.Incompatibles.Addons];
        OptionsList = addon.Options is null ? null : [.. addon.Options];

        if (addon.StartMap is MapFileJsonModel mapFile)
        {
            IsFileMapTypeSelected = true;
            MapFileName = mapFile.File;
        }
        else if (addon.StartMap is MapSlotJsonModel slotFile)
        {
            IsElMapTypeSelected = true;
            MapEpisode = slotFile.Episode;
            MapLevel = slotFile.Level;
        }

        AddonDescription = addon.Description;

        if (addon.Executables is not null)
        {
            if (addon.Executables.TryGetValue(OSEnum.Windows, out var windowsExes))
            {
                _ = windowsExes.TryGetValue(PortEnum.EDuke32, out var exe1);
                WindowsEDukeExe = exe1;
                _ = windowsExes.TryGetValue(PortEnum.NBlood, out var exe2);
                WindowsNBloodExe = exe2;
                _ = windowsExes.TryGetValue(PortEnum.NotBlood, out var exe3);
                WindowsNotBloodExe = exe3;
            }

            if (addon.Executables.TryGetValue(OSEnum.Linux, out var linuxExes))
            {
                _ = linuxExes.TryGetValue(PortEnum.EDuke32, out var exe1);
                LinuxEDukeExe = exe1;
                _ = linuxExes.TryGetValue(PortEnum.NBlood, out var exe2);
                LinuxNBloodExe = exe2;
                _ = linuxExes.TryGetValue(PortEnum.NotBlood, out var exe3);
                LinuxNotBloodExe = exe3;
            }
        }
        else
        {
            WindowsEDukeExe = null;
            WindowsNBloodExe = null;
            WindowsNotBloodExe = null;
            LinuxEDukeExe = null;
            LinuxNBloodExe = null;
            LinuxNotBloodExe = null;
        }
    }

    /// <summary>
    ///     Renames the addon folder to match the addon's full name.
    /// </summary>
    /// <param name="addon">The addon manifest.</param>
    private void RenameAddonFolder(AddonManifestJsonModel addon)
    {
        ArgumentNullException.ThrowIfNull(PathToAddonFolder);

        var fullName = GetAddonFullName(addon);
        var parentDir = Path.GetDirectoryName(PathToAddonFolder) ?? throw new InvalidOperationException("Could not determine parent directory for addon folder");
        var newFolderPath = Path.Combine(parentDir, fullName);

        if (!PathToAddonFolder.Equals(newFolderPath))
        {
            Directory.Move(PathToAddonFolder, newFolderPath);
            PathToAddonFolder = newFolderPath;
        }
    }

    /// <summary>
    ///     Gets the full folder name for an addon based on its ID and version.
    /// </summary>
    /// <param name="addon">The addon manifest.</param>
    /// <returns>The full folder name.</returns>
    private static string GetAddonFullName(AddonManifestJsonModel addon)
    {
        StringBuilder version = new();

        foreach (var ch in addon.Version)
        {
            if (Path.GetInvalidPathChars().Contains(ch) ||
                ch == '.')
            {
                _ = version.Append('_');
            }
            else
            {
                _ = version.Append(ch);
            }
        }

        return $"{addon.Id}_v{version}";
    }

    /// <summary>
    ///     Sets the result message on the UI thread.
    /// </summary>
    /// <param name="message">The message text.</param>
    /// <param name="isError">Whether this is an error message.</param>
    private void SetResultMessage(string message, bool isError)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ErrorTextColor = isError ? SolidColorBrush.Parse("Red") : SolidColorBrush.Parse("Green");
            ErrorText = message;
        });
    }

    /// <summary>
    ///     Create zip archive with addon files.
    /// </summary>
    private async Task<string?> CreateZipInternalAsync()
    {
        try
        {
            IsInProgress = true;

            if (string.IsNullOrWhiteSpace(PathToAddonFolder) &&
                !Directory.Exists(PathToAddonFolder))
            {
                SetResultMessage("Choose addon folder", true);

                return null;
            }

            SaveJson();

            var addon = GetAddonJson(out var jsonString);

            ArgumentNullException.ThrowIfNull(PathToAddonFolder);
            File.WriteAllText(Path.Combine(PathToAddonFolder, "addon.json"), jsonString);

            string archiveSaveFolder;
            var game = _gamesProvider.GetGame(addon.SupportedGame.Game);

            if (addon.AddonType is AddonTypeEnum.TC)
            {
                archiveSaveFolder = game.CampaignsFolderPath;
            }
            else if (addon.AddonType is AddonTypeEnum.Mod)
            {
                archiveSaveFolder = game.ModsFolderPath;
            }
            else if (addon.AddonType is AddonTypeEnum.Map)
            {
                archiveSaveFolder = game.MapsFolderPath;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(addon.AddonType));
            }

            var fullName = GetAddonFullName(addon);
            var pathToArchive = Path.Combine(archiveSaveFolder, fullName + ".zip");

            SetResultMessage("Making zip. Please wait.", false);

            List<FileStream> fileStreams = new();

            using (var archive = ZipArchive.CreateArchive())
            {
                using (archive.PauseEntryRebuilding())
                {
                    var files = Directory.EnumerateFiles(PathToAddonFolder, "*.*", SearchOption.AllDirectories).ToList();
                    var filesCount = files.Count;
                    var currentFile = 0;

                    foreach (var path in files)
                    {
                        var fileInfo = new FileInfo(path);
                        var fileStream = fileInfo.OpenRead();
                        fileStreams.Add(fileStream);

                        _ = archive.AddEntry(
                            path[PathToAddonFolder.Length..],
                            fileStream,
                            true,
                            fileInfo.Length,
                            fileInfo.LastWriteTime
                            );

                        currentFile++;
                        ProgressBarValue = (int)(currentFile / (double)filesCount * 100);
                    }
                }

                await Task.Run(() => archive.SaveTo(pathToArchive, CompressionType.None)).ConfigureAwait(false);

                fileStreams.ForEach(x => x.Dispose());
            }

            SetResultMessage("Zip created successfully. Go to the game page and press Refresh.", false);

            return pathToArchive;
        }
        catch (Exception ex)
        {
            SetResultMessage(ex.Message, true);

            return null;
        }
        finally
        {
            IsInProgress = false;
        }
    }


    #region Binding Properties

    /// <summary>
    ///     Use local API parameter
    /// </summary>
    public bool LocalApiCheckbox
    {
        get => _config.UseLocalApi;
        set
        {
            _config.UseLocalApi = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets the addon ID prefix for the selected game.
    /// </summary>
    public string AddonIdPrefix => SelectedGame switch
    {
        GameEnum.Duke3D => "duke3d-",
        GameEnum.Blood => "blood-",
        GameEnum.Wang => "wang-",
        GameEnum.Fury => "fury-",
        GameEnum.Redneck => "redneck-",
        GameEnum.RidesAgain => "ridesagain-",
        GameEnum.Slave => "slave-",
        GameEnum.NAM => "nam-",
        GameEnum.WW2GI => "ww2gi-",
        GameEnum.Witchaven => "wh1-",
        GameEnum.Witchaven2 => "wh2-",
        _ => string.Empty
    };

    /// <summary>
    ///     Gets whether the app is running in developer mode.
    /// </summary>
    public bool IsDeveloperMode => ClientProperties.IsDeveloperMode;

    /// <summary>
    ///     Gets whether step 2 (addon type selection) is visible.
    /// </summary>
    public bool IsStep2Visible => IsMapSelected || IsModSelected || IsTcSelected;

    /// <summary>
    ///     Gets whether step 3 (game selection) is visible.
    /// </summary>
    public bool IsStep3Visible => SelectedGame is not null;

    /// <summary>
    ///     Gets whether Duke-specific properties are available.
    /// </summary>
    public bool AreDukePropertiesAvailable => SelectedGame is GameEnum.Duke3D or GameEnum.Fury or GameEnum.Redneck or GameEnum.RidesAgain or GameEnum.NAM or GameEnum.WW2GI;

    /// <summary>
    ///     Gets whether the main CON field is available.
    /// </summary>
    public bool IsMainConAvailable => AreDukePropertiesAvailable && !IsModSelected;

    /// <summary>
    ///     Gets whether Blood-specific properties are available.
    /// </summary>
    public bool AreBloodPropertiesAvailable => SelectedGame is GameEnum.Blood;

    /// <summary>
    ///     Gets or sets the addon ID.
    /// </summary>
    [ObservableProperty]
    private string _addonId = string.Empty;

    /// <summary>
    ///     Gets or sets the addon version.
    /// </summary>
    [ObservableProperty]
    private string _addonVersion = string.Empty;

    /// <summary>
    ///     Gets or sets the release date.
    /// </summary>
    [ObservableProperty]
    private DateTimeOffset? _releaseDate;

    /// <summary>
    ///     Gets or sets the addon author.
    /// </summary>
    [ObservableProperty]
    private string? _addonAuthor;

    /// <summary>
    ///     Gets or sets the addon description.
    /// </summary>
    [ObservableProperty]
    private string? _addonDescription;

    /// <summary>
    ///     Gets or sets the main DEF file.
    /// </summary>
    [ObservableProperty]
    private string? _mainDef;

    /// <summary>
    ///     Gets or sets the additional DEF files.
    /// </summary>
    [ObservableProperty]
    private string? _additionalDefs;

    /// <summary>
    ///     Gets or sets the main CON file.
    /// </summary>
    [ObservableProperty]
    private string? _mainCon;

    /// <summary>
    ///     Gets or sets the additional CON files.
    /// </summary>
    [ObservableProperty]
    private string? _additionalCons;

    /// <summary>
    ///     Gets or sets the RTS file.
    /// </summary>
    [ObservableProperty]
    private string? _rts;

    /// <summary>
    ///     Gets or sets the INI file.
    /// </summary>
    [ObservableProperty]
    private string? _ini;

    /// <summary>
    ///     Gets or sets the main RFF file.
    /// </summary>
    [ObservableProperty]
    private string? _mainRff;

    /// <summary>
    ///     Gets or sets the sound RFF file.
    /// </summary>
    [ObservableProperty]
    private string? _soundRff;

    /// <summary>
    ///     Gets or sets the map file name.
    /// </summary>
    [ObservableProperty]
    private string? _mapFileName;

    /// <summary>
    ///     Gets or sets the Windows EDuke32 executable path.
    /// </summary>
    [ObservableProperty]
    private string? _windowsEDukeExe;

    /// <summary>
    ///     Gets or sets the Windows NBlood executable path.
    /// </summary>
    [ObservableProperty]
    private string? _windowsNBloodExe;

    /// <summary>
    ///     Gets or sets the Windows NotBlood executable path.
    /// </summary>
    [ObservableProperty]
    private string? _windowsNotBloodExe;

    /// <summary>
    ///     Gets or sets the Windows RedNukem executable path.
    /// </summary>
    [ObservableProperty]
    private string? _windowsRedNukemExe;

    /// <summary>
    ///     Gets or sets the Windows Raze executable path.
    /// </summary>
    [ObservableProperty]
    private string? _windowsRazeExe;

    /// <summary>
    ///     Gets or sets the Windows PCExhumed executable path.
    /// </summary>
    [ObservableProperty]
    private string? _windowsPCExhumedExe;

    /// <summary>
    ///     Gets or sets the Linux NBlood executable path.
    /// </summary>
    [ObservableProperty]
    private string? _linuxNBloodExe;

    /// <summary>
    ///     Gets or sets the Linux RedNukem executable path.
    /// </summary>
    [ObservableProperty]
    private string? _linuxRedNukemExe;

    /// <summary>
    ///     Gets or sets the Linux Raze executable path.
    /// </summary>
    [ObservableProperty]
    private string? _linuxRazeExe;

    /// <summary>
    ///     Gets or sets the Linux PCExhumed executable path.
    /// </summary>
    [ObservableProperty]
    private string? _linuxPCExhumedExe;

    /// <summary>
    ///     Gets or sets the Linux NotBlood executable path.
    /// </summary>
    [ObservableProperty]
    private string? _linuxNotBloodExe;

    /// <summary>
    ///     Gets or sets the Linux EDuke32 executable path.
    /// </summary>
    [ObservableProperty]
    private string? _linuxEDukeExe;

    /// <summary>
    ///     Gets or sets the map episode number.
    /// </summary>
    [ObservableProperty]
    private int? _mapEpisode;

    /// <summary>
    ///     Gets or sets the map level number.
    /// </summary>
    [ObservableProperty]
    private int? _mapLevel;
    /// <summary>
    ///     Gets or sets whether the Duke Atomic version is selected.
    /// </summary>
    [ObservableProperty]
    private bool _isDukeAtomicSelected;

    /// <summary>
    ///     Gets or sets whether the Duke 1.3D version is selected.
    /// </summary>
    [ObservableProperty]
    private bool _isDuke13DSelected;

    /// <summary>
    ///     Gets or sets whether the Duke Widescreen version is selected.
    /// </summary>
    [ObservableProperty]
    private bool _isDukeWTSelected;

    /// <summary>
    ///     Gets or sets whether EDuke32 CON features are selected.
    /// </summary>
    [ObservableProperty]
    private bool _isEdukeConsSelected;

    /// <summary>
    ///     Gets or sets whether modern types are selected.
    /// </summary>
    [ObservableProperty]
    private bool _isModernTypesSelected;

    /// <summary>
    ///     Gets or sets whether models are selected.
    /// </summary>
    [ObservableProperty]
    private bool _isModelsSelected;

    /// <summary>
    ///     Gets or sets whether hightile textures are selected.
    /// </summary>
    [ObservableProperty]
    private bool _isHightileSelected;

    /// <summary>
    ///     Gets or sets whether sloped sprites are selected.
    /// </summary>
    [ObservableProperty]
    private bool _isSlopedSelected;

    /// <summary>
    ///     Gets or sets whether TROR is selected.
    /// </summary>
    [ObservableProperty]
    private bool _isTrorSelected;

    /// <summary>
    ///     Gets or sets whether wall rotate cstat is selected.
    /// </summary>
    [ObservableProperty]
    private bool _isCstatSelected;

    /// <summary>
    ///     Gets or sets whether dynamic lighting is selected.
    /// </summary>
    [ObservableProperty]
    private bool _isLightingSelected;

    /// <summary>
    ///     Gets or sets whether SndInfo is selected.
    /// </summary>
    [ObservableProperty]
    private bool _isSndInfoSelected;

    /// <summary>
    ///     Gets or sets whether tile from texture is selected.
    /// </summary>
    [ObservableProperty]
    private bool _isTilefromtextureSelected;

    /// <summary>
    ///     Gets or sets whether an operation is in progress.
    /// </summary>
    [ObservableProperty]
    private bool _isInProgress;

    /// <summary>
    ///     Gets or sets the progress bar value.
    /// </summary>
    [ObservableProperty]
    private int _progressBarValue = 0;

    /// <summary>
    ///     Gets or sets the addon title.
    /// </summary>
    [ObservableProperty]
    private string _addonTitle = string.Empty;

    /// <summary>
    ///     Called when the addon title changes; auto-generates the addon ID.
    /// </summary>
    partial void OnAddonTitleChanged(string value)
    {
        StringBuilder sb = new(value.Length);

        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
            {
                _ = sb.Append(char.ToLower(ch));
            }
            else if (char.IsWhiteSpace(ch))
            {
                _ = sb.Append("-");
            }
        }

        AddonId = sb.ToString();
        OnPropertyChanged(nameof(AddonId));
    }


    [NotifyPropertyChangedFor(nameof(AreDukePropertiesAvailable))]
    [NotifyPropertyChangedFor(nameof(IsMainConAvailable))]
    [NotifyPropertyChangedFor(nameof(AddonIdPrefix))]
    [NotifyPropertyChangedFor(nameof(IsStep3Visible))]
    [NotifyPropertyChangedFor(nameof(AreBloodPropertiesAvailable))]
    [NotifyPropertyChangedFor(nameof(WindowsNBloodExe))]
    [NotifyPropertyChangedFor(nameof(WindowsNotBloodExe))]
    [NotifyPropertyChangedFor(nameof(WindowsEDukeExe))]
    [NotifyPropertyChangedFor(nameof(LinuxNBloodExe))]
    [NotifyPropertyChangedFor(nameof(LinuxNotBloodExe))]
    [NotifyPropertyChangedFor(nameof(LinuxEDukeExe))]
    /// <summary>
    ///     Gets or sets the selected game.
    /// </summary>
    [ObservableProperty]
    private GameEnum? _selectedGame;

    /// <summary>
    ///     Gets or sets whether the map addon type is selected.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStep2Visible))]
    private bool _isMapSelected;

    /// <summary>
    ///     Gets or sets whether the TC addon type is selected.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStep2Visible))]
    private bool _isTcSelected;

    /// <summary>
    ///     Gets or sets whether the mod addon type is selected.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMainConAvailable))]
    [NotifyPropertyChangedFor(nameof(IsStep2Visible))]
    private bool _isModSelected;

    /// <summary>
    ///     Gets or sets whether the EL map type is selected.
    /// </summary>
    [ObservableProperty]
    private bool _isElMapTypeSelected;

    /// <summary>
    ///     Gets or sets whether the file map type is selected.
    /// </summary>
    [ObservableProperty]
    private bool _isFileMapTypeSelected;

    /// <summary>
    ///     Gets or sets the uploading status message.
    /// </summary>
    [ObservableProperty]
    private string? _uploadingStatusMessage;

    /// <summary>
    ///     Gets or sets the error text.
    /// </summary>
    [ObservableProperty]
    private string? _errorText;

    /// <summary>
    ///     Gets or sets the error text color.
    /// </summary>
    [ObservableProperty]
    private SolidColorBrush _errorTextColor = SolidColorBrush.Parse("Red");

    /// <summary>
    ///     Gets or sets the path to the addon folder.
    /// </summary>
    [ObservableProperty]
    private string? _pathToAddonFolder;

    /// <summary>
    ///     Called when the addon folder path changes; loads addon.json if it exists.
    /// </summary>
    partial void OnPathToAddonFolderChanged(string? value)
    {
        if (value is null)
        {
            return;
        }

        if (!Directory.Exists(value))
        {
            SetResultMessage("Folder doesn't exist", true);

            return;
        }

        ErrorText = null;

        var addonJson = Path.Combine(value, "addon.json");

        if (File.Exists(addonJson))
        {
            LoadJson(addonJson);
        }

        OnPropertyChanged(nameof(SelectedGame));
    }

    /// <summary>
    ///     Gets or sets the game CRC.
    /// </summary>
    [ObservableProperty]
    private string? _gameCrc;

    /// <summary>
    ///     Gets or sets the JSON text.
    /// </summary>
    [ObservableProperty]
    private string? _jsonText;

    /// <summary>
    ///     Gets or sets the upload status.
    /// </summary>
    [ObservableProperty]
    private string? _uploadStatus;

    /// <summary>
    ///     Gets or sets the API password text box value.
    /// </summary>
    [ObservableProperty]
    private string _apiPasswordTextBox;

    /// <summary>
    ///     Called when the API password changes; updates the configuration.
    /// </summary>
    partial void OnApiPasswordTextBoxChanged(string value)
    {
        var configValue = _config.ApiPassword;

        if (value.Equals(configValue))
        {
            return;
        }
        else
        {
            _config.ApiPassword = value;
        }
    }

    /// <summary>
    ///     Gets or sets the list of dependencies.
    /// </summary>
    [ObservableProperty]
    private ImmutableList<DependantAddonJsonModel>? _dependenciesList;

    /// <summary>
    ///     Gets or sets the list of incompatibilities.
    /// </summary>
    [ObservableProperty]
    private ImmutableList<DependantAddonJsonModel>? _incompatibilitiesList;

    /// <summary>
    ///     Gets or sets the list of options.
    /// </summary>
    [ObservableProperty]
    public partial ImmutableList<OptionJsonModel>? OptionsList { get; set; }

    #endregion


    #region Relay Commands

    /// <summary>
    ///     Opens a folder picker to select the addon folder.
    /// </summary>
    [RelayCommand]
    private async Task SelectAddonFolder()
    {
        var folders = await AvaloniaProperties.TopLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Choose addon folder",
                AllowMultiple = false
            }).ConfigureAwait(true);

        if (folders.Count == 0)
        {
            return;
        }

        PathToAddonFolder = folders[0].Path.LocalPath;
    }


    /// <summary>
    ///     Uploads and adds an addon to the database.
    /// </summary>
    [RelayCommand]
    private async Task AddAddonAsync()
    {
        FilePickerFileType z64 = new("Zipped addon")
        {
            Patterns = ["*.zip"]
        };

        var files = await AvaloniaProperties.TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Choose addon file",
                AllowMultiple = true,
                FileTypeFilter = [z64]
            }).ConfigureAwait(true);

        if (files.Count == 0)
        {
            return;
        }

        StringBuilder errors = new();

        try
        {
            SetResultMessage("Uploading file. Please wait.", false);
            IsInProgress = true;

            StrongBox<int> progress = new(ProgressBarValue);

            _ = Task.Run(async () =>
            {
                while (IsInProgress)
                {
                    ProgressBarValue = progress.Value;
                    await Task.Delay(50).ConfigureAwait(false);
                }
            });

            foreach (var file in files)
            {
                var pathToFile = file.Path.LocalPath;
                var manifestResult = await ManifestHelper.GetMainManifestAsync(pathToFile).ConfigureAwait(false);

                if (!manifestResult.IsSuccess)
                {
                    _ = errors.AppendLine($"Error while adding {file.Path.AbsolutePath}: {manifestResult.Message}");

                    continue;
                }

                var fileS3Key = UriHelper.GetRelativeFilePath(manifestResult.ResultObject, pathToFile);

                var uploadResult = await _filesUploader.UploadFileAsync(pathToFile, fileS3Key, progress, CancellationToken.None).ConfigureAwait(true);

                if (!uploadResult.IsSuccess)
                {
                    _ = errors.AppendLine($"Error while adding {file.Path.AbsolutePath}: {uploadResult.Message}");

                    continue;
                }

                var addingResult = await _addonsDatabaseManager.AddToDatabaseAsync(pathToFile, uploadResult.ResultObject.Value.Url, manifestResult.ResultObject).ConfigureAwait(false);

                if (!addingResult.IsSuccess)
                {
                    _ = errors.AppendLine($"Error while adding {file.Path.AbsolutePath}: {addingResult.Message}");

                    continue;
                }
            }

            if (errors.Length > 0)
            {
                SetResultMessage(errors.ToString(), true);
            }
            else
            {
                SetResultMessage("Success", false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error while uploading addon");
            SetResultMessage(ex.ToString(), true);
        }
        finally
        {
            IsInProgress = false;
            ProgressBarValue = 0;
        }
    }


    /// <summary>
    ///     Adds a new dependency entry.
    /// </summary>
    [RelayCommand]
    private void AddDependency()
    {
        DependantAddonJsonModel newAddon = new()
        {
            Id = "",
            Version = null
        };

        DependenciesList ??= [];

        DependenciesList = DependenciesList.Add(newAddon);
    }

    /// <summary>
    ///     Removes a dependency entry.
    /// </summary>
    [RelayCommand]
    private void RemoveDependency(DependantAddonJsonModel dependency)
    {
        ArgumentNullException.ThrowIfNull(DependenciesList);

        DependenciesList = DependenciesList.Remove(dependency);
    }


    /// <summary>
    ///     Adds a new incompatibility entry.
    /// </summary>
    [RelayCommand]
    private void AddIncompatibility()
    {
        DependantAddonJsonModel newAddon = new()
        {
            Id = "",
            Version = null
        };

        IncompatibilitiesList ??= [];

        IncompatibilitiesList = IncompatibilitiesList.Add(newAddon);
    }


    /// <summary>
    ///     Removes an incompatibility entry.
    /// </summary>
    [RelayCommand]
    private void RemoveIncompatibility(DependantAddonJsonModel dependency)
    {
        ArgumentNullException.ThrowIfNull(IncompatibilitiesList);

        IncompatibilitiesList = IncompatibilitiesList.Remove(dependency);
    }


    /// <summary>
    ///     Adds a new option entry.
    /// </summary>
    [RelayCommand]
    private void AddOption()
    {
        OptionJsonModel newOption = new()
        {
            OptionName = "",
            Parameters = []
        };

        OptionsList ??= [];

        OptionsList = OptionsList.Add(newOption);
    }

    /// <summary>
    ///     Removes an option entry.
    /// </summary>
    [RelayCommand]
    private void RemoveOption(OptionJsonModel option)
    {
        ArgumentNullException.ThrowIfNull(OptionsList);

        OptionsList = OptionsList.Remove(option);
    }


    /// <summary>
    ///     Opens a file picker to select a file for CRC calculation.
    /// </summary>
    [RelayCommand]
    private async Task SelectFileForCrcAsync()
    {
        var files = await AvaloniaProperties.TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Choose file",
                AllowMultiple = false
            }).ConfigureAwait(true);

        if (files.Count == 0)
        {
            return;
        }

        GameCrc = Crc32Helper.GetCrc32Hex(files[0].Path.LocalPath);
    }

    /// <summary>
    ///     Creates a zip archive of the addon.
    /// </summary>
    [RelayCommand]
    private Task<string?> CreateZipAsync() => CreateZipInternalAsync();


    /// <summary>
    ///     Submits the addon for upload.
    /// </summary>
    [RelayCommand]
    private async Task SubmitAddonAsync()
    {
        var pathToArchive = await CreateZipInternalAsync().ConfigureAwait(true);

        if (pathToArchive is null)
        {
            return;
        }

        try
        {
            SetResultMessage("Uploading file. Please wait.", false);
            IsInProgress = true;

            StrongBox<int> progress = new(ProgressBarValue);

            _ = Task.Run(async () =>
            {
                while (IsInProgress)
                {
                    ProgressBarValue = progress.Value;
                    await Task.Delay(50).ConfigureAwait(false);
                }
            });

            var uploadResult = await _filesUploader.UploadFileAsync(
                pathToArchive,
                $"uploads/{Guid.NewGuid()}/{Path.GetFileName(pathToArchive)}",
                progress,
                CancellationToken.None
                ).ConfigureAwait(false);

            if (!uploadResult.IsSuccess)
            {
                SetResultMessage(uploadResult.Message, true);
            }
            else
            {
                SetResultMessage("Uploaded successfully", false);
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Uploading cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error while uploading file");
            SetResultMessage(ex.ToString(), true);
        }
        finally
        {
            IsInProgress = false;
            ProgressBarValue = 0;
        }
    }


    /// <summary>
    ///     Previews the generated JSON for the addon.
    /// </summary>
    [RelayCommand]
    private void PreviewJson()
    {
        try
        {
            _ = GetAddonJson(out _);
        }
        catch (Exception ex)
        {
            SetResultMessage(ex.Message, true);
        }
    }

    /// <summary>
    ///     Saves the JSON manifest to the addon folder.
    /// </summary>
    [RelayCommand]
    private void SaveJson()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(PathToAddonFolder) &&
                !Directory.Exists(PathToAddonFolder))
            {
                SetResultMessage("Choose addon folder", true);

                return;
            }

            var addon = GetAddonJson(out var jsonString);

            ArgumentNullException.ThrowIfNull(PathToAddonFolder);
            File.WriteAllText(Path.Combine(PathToAddonFolder, "addon.json"), jsonString);

            RenameAddonFolder(addon);

            SetResultMessage("JSON saved", false);
        }
        catch (Exception ex)
        {
            SetResultMessage(ex.Message, true);
        }
    }

    /// <summary>
    ///     Updates all addon manifests from the selected folder.
    /// </summary>
    [RelayCommand]
    private async Task UpdateManifestsAsync()
    {
        if (ClientProperties.PathToLocalManifestsJson is null)
        {
            throw new InvalidOperationException($"{nameof(ClientProperties.PathToLocalManifestsJson)} is null");
        }

        var folders = await AvaloniaProperties.TopLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Choose addons folder",
                AllowMultiple = false
            }).ConfigureAwait(true);

        if (folders.Count == 0)
        {
            return;
        }

        var files = Directory.EnumerateFiles(folders[0].Path.AbsolutePath, "*.zip", SearchOption.AllDirectories).ToList();

        if (files is null or [])
        {
            return;
        }

        List<AddonManifestJsonModel> result = new(files.Count);

        foreach (var file in files)
        {
            using var archive = ArchiveFactory.OpenArchive(file);
            var jsons = archive.Entries.Where(x => x.Key?.StartsWith("addon", StringComparison.OrdinalIgnoreCase) == true && x.Key.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

            foreach (var json in jsons)
            {
                using var jsonStream = await json.OpenEntryStreamAsync().ConfigureAwait(false);

                var jsonStr = await JsonSerializer.DeserializeAsync(
                    jsonStream,
                    AddonManifestJsonContext.Default.AddonManifestJsonModel
                    ).ConfigureAwait(false);

                if (jsonStr is null)
                {
                    continue;
                }

                result.Add(jsonStr);
            }
        }

        var list = JsonSerializer.Serialize(result, AddonManifestJsonContext.Default.ListAddonManifestJsonModel);
        await File.WriteAllTextAsync(ClientProperties.PathToLocalManifestsJson, list).ConfigureAwait(false);
    }

    #endregion
}
