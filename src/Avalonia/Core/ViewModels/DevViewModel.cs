using Avalonia.Platform.Storage;
using BuildLauncher.Helpers;
using Common.Client.Config;
using Common.Client.Helpers;
using Common.Enums;
using Common.Enums.Versions;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Games.Providers;
using Mods;
using Mods.Serializable;
using Mods.Serializable.Addon;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace BuildLauncher.ViewModels;

public sealed partial class DevViewModel : ObservableObject
{
    private readonly IConfigProvider _config;
    private readonly FilesUploader _filesUploader;
    private readonly GamesProvider _gamesProvider;

    private readonly List<string> _forbiddenNames =
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

    public DevViewModel(
        IConfigProvider config,
        FilesUploader filesUploader,
        GamesProvider gamesProvider
        )
    {
        _config = config;
        _filesUploader = filesUploader;
        _gamesProvider = gamesProvider;

        ApiPasswordTextBox = _config.ApiPassword;
    }


    #region Binding Properties

    /// <summary>
    /// Use local API parameter
    /// </summary>
    public bool LocalApiCheckbox
    {
        get => _config.UseLocalApi;
        set
        {
            _config.UseLocalApi = value;
            OnPropertyChanged(nameof(LocalApiCheckbox));
        }
    }

    public string AddonIdPrefix
    {
        get
        {
            if (IsDukeSelected)
            {
                return "duke3d-";
            }
            if (IsBloodSelected)
            {
                return "blood-";
            }
            if (IsWangSelected)
            {
                return "wang-";
            }
            if (IsFurySelected)
            {
                return "fury-";
            }
            if (IsRedneckSelected)
            {
                return "redneck-";
            }
            if (IsRidesAgainSelected)
            {
                return "ridesagain-";
            }
            if (IsSlaveSelected)
            {
                return "slave-";
            }

            return string.Empty;
        }
    }

    public bool IsDevMode => ClientProperties.IsDevMode;
    public bool IsStep2Visible => IsMapSelected || IsModSelected || IsTcSelected;
    public bool IsStep3Visible => IsDukeSelected || IsBloodSelected || IsWangSelected || IsFurySelected || IsRedneckSelected || IsRidesAgainSelected || IsSlaveSelected;
    public bool AreDukePropertiesAvailable => IsDukeSelected || IsFurySelected || IsRedneckSelected;
    public bool IsMainConAvailable => AreDukePropertiesAvailable && !IsModSelected;
    public bool AreBloodPropertiesVisible => IsBloodSelected;

    [ObservableProperty]
    private string _addonId;
    [ObservableProperty]
    private string _addonVersion;
    [ObservableProperty]
    private string? _addonAuthor;
    [ObservableProperty]
    private string? _addonDescription;
    [ObservableProperty]
    private string? _mainDef;
    [ObservableProperty]
    private string? _additionalDefs;
    [ObservableProperty]
    private string? _mainCon;
    [ObservableProperty]
    private string? _additionalCons;
    [ObservableProperty]
    private string? _rts;
    [ObservableProperty]
    private string? _ini;
    [ObservableProperty]
    private string? _mainRff;
    [ObservableProperty]
    private string? _soundRff;
    [ObservableProperty]
    private string? _mapFileName;
    [ObservableProperty]
    private int? _mapEpisode;
    [ObservableProperty]
    private int? _mapLevel;
    [ObservableProperty]
    private bool _isDukeAtomicSelected;
    [ObservableProperty]
    private bool _isDuke13DSelected;
    [ObservableProperty]
    private bool _isDukeWTSelected;
    [ObservableProperty]
    private bool _isEdukeConsSelected;
    [ObservableProperty]
    private bool _isModernTypesSelected;
    [ObservableProperty]
    private bool _isModelsSelected;
    [ObservableProperty]
    private bool _isHightileSelected;
    [ObservableProperty]
    private bool _isSlopedSelected;
    [ObservableProperty]
    private bool _isTrorSelected;
    [ObservableProperty]
    private bool _isCstatSelected;
    [ObservableProperty]
    private bool _isLightingSelected;

    [ObservableProperty]
    private string _addonTitle;
    partial void OnAddonTitleChanged(string value)
    {
        StringBuilder sb = new(value.Count());

        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(char.ToLower(ch));
            }
            else if (char.IsWhiteSpace(ch))
            {
                sb.Append("-");
            }
        }

        AddonId = sb.ToString();
        OnPropertyChanged(nameof(AddonId));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AreDukePropertiesAvailable))]
    [NotifyPropertyChangedFor(nameof(IsMainConAvailable))]
    [NotifyPropertyChangedFor(nameof(AddonIdPrefix))]
    [NotifyPropertyChangedFor(nameof(IsStep3Visible))]
    private bool _isDukeSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AreBloodPropertiesVisible))]
    [NotifyPropertyChangedFor(nameof(AddonIdPrefix))]
    [NotifyPropertyChangedFor(nameof(IsStep3Visible))]
    private bool _isBloodSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AddonIdPrefix))]
    [NotifyPropertyChangedFor(nameof(IsStep3Visible))]
    private bool _isWangSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AreDukePropertiesAvailable))]
    [NotifyPropertyChangedFor(nameof(IsMainConAvailable))]
    [NotifyPropertyChangedFor(nameof(AddonIdPrefix))]
    [NotifyPropertyChangedFor(nameof(IsStep3Visible))]
    private bool _isFurySelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AreDukePropertiesAvailable))]
    [NotifyPropertyChangedFor(nameof(IsMainConAvailable))]
    [NotifyPropertyChangedFor(nameof(AddonIdPrefix))]
    [NotifyPropertyChangedFor(nameof(IsStep3Visible))]
    private bool _isRedneckSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AreDukePropertiesAvailable))]
    [NotifyPropertyChangedFor(nameof(IsMainConAvailable))]
    [NotifyPropertyChangedFor(nameof(AddonIdPrefix))]
    [NotifyPropertyChangedFor(nameof(IsStep3Visible))]
    private bool _isRidesAgainSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AddonIdPrefix))]
    [NotifyPropertyChangedFor(nameof(IsStep3Visible))]
    private bool _isSlaveSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStep2Visible))]
    private bool _isMapSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStep2Visible))]
    private bool _isTcSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMainConAvailable))]
    [NotifyPropertyChangedFor(nameof(IsStep2Visible))]
    private bool _isModSelected;

    [ObservableProperty]
    private bool _isElMapTypeSelected;

    [ObservableProperty]
    private bool _isFileMapTypeSelected;

    [ObservableProperty]
    private string? _uploadingStatusMessage;

    [ObservableProperty]
    private string? _errorText;

    [ObservableProperty]
    private string? _pathToAddonFolder;
    partial void OnPathToAddonFolderChanged(string? value)
    {
        if (value is null)
        {
            return;
        }

        if (!Directory.Exists(value))
        {
            ErrorText = "Folder doesn't exist";
            return;
        }

        ErrorText = null;

        var addonJson = Path.Combine(value, "addon.json");

        if (File.Exists(addonJson))
        {
            LoadJson(addonJson);
        }
    }

    [ObservableProperty]
    private string? _gameCrc;

    [ObservableProperty]
    private string? _jsonText;

    [ObservableProperty]
    private string? _uploadStatus;

    [ObservableProperty]
    private string _apiPasswordTextBox;
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

    [ObservableProperty]
    private ImmutableList<DependantAddonDto> _dependenciesList;

    [ObservableProperty]
    private ImmutableList<DependantAddonDto> _incompatibilitiesList;

    #endregion


    #region Relay Commands

    [RelayCommand]
    private async Task SelectAddonFolder()
    {
        var folders = await Properties.TopLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Choose addon folder",
                AllowMultiple = false
            }).ConfigureAwait(true);

        if (folders.Count == 0)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task AddAddonAsync()
    {
        FilePickerFileType z64 = new("Zipped addon")
        {
            Patterns = ["*.zip"]
        };

        var files = await Properties.TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Choose addon file",
                AllowMultiple = false,
                FileTypeFilter = [z64]
            }).ConfigureAwait(true);

        if (files.Count == 0)
        {
            return;
        }

        var result = await _filesUploader.AddAddonToDatabaseAsync(files[0].Path.LocalPath);

        if (result)
        {
            UploadingStatusMessage = "Success";
        }
        else
        {
            UploadingStatusMessage = "Error";
        }
    }

    [RelayCommand]
    private async Task LoadJson()
    {
        FilePickerFileType addonJson = new("addon.json")
        {
            Patterns = ["addon.json"]
        };

        var files = await Properties.TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Choose addon.json",
                AllowMultiple = false,
                FileTypeFilter = [addonJson]
            }).ConfigureAwait(true);

        if (files.Count == 0)
        {
            return;
        }

        LoadJson(files[0].Path.LocalPath);
    }

    private void LoadJson(string pathToFile)
    {
        var jsonStr = File.ReadAllText(pathToFile);

        var result = JsonSerializer.Deserialize(jsonStr, AddonManifestContext.Default.AddonDto);

        if (result is null)
        {
            return;
        }

        IsTcSelected = result.AddonType is AddonTypeEnum.TC;
        IsMapSelected = result.AddonType is AddonTypeEnum.Map;
        IsModSelected = result.AddonType is AddonTypeEnum.Mod;

        IsDukeSelected = result.SupportedGame.Game is GameEnum.Duke3D;
        IsBloodSelected = result.SupportedGame.Game is GameEnum.Blood;
        IsWangSelected = result.SupportedGame.Game is GameEnum.ShadowWarrior;
        IsFurySelected = result.SupportedGame.Game is GameEnum.Fury;
        IsRedneckSelected = result.SupportedGame.Game is GameEnum.Redneck;
        IsRidesAgainSelected = result.SupportedGame.Game is GameEnum.RidesAgain;
        IsSlaveSelected = result.SupportedGame.Game is GameEnum.Exhumed;

        var isDukeVersion = Enum.TryParse<DukeVersionEnum>(result.SupportedGame.Version, true, out var dukeVersion);
        IsDuke13DSelected = isDukeVersion && dukeVersion is DukeVersionEnum.Duke3D_13D;
        IsDukeAtomicSelected = isDukeVersion && dukeVersion is DukeVersionEnum.Duke3D_Atomic;
        IsDukeWTSelected = isDukeVersion && dukeVersion is DukeVersionEnum.Duke3D_WT;

        GameCrc = result.SupportedGame.Crc;

        AddonTitle = result.Title;
        AddonId = result.Id.Replace(AddonIdPrefix, "");
        AddonVersion = result.Version;
        AddonAuthor = result.Author;
        MainDef = result.MainDef;
        AdditionalDefs = result.AdditionalDefs is null? null : string.Join(',', result.AdditionalDefs);
        MainCon = result.MainCon;
        AdditionalCons = result.AdditionalCons is null? null : string.Join(',', result.AdditionalCons);
        Rts = result.Rts;
        Ini = result.Ini;
        MainRff = result.MainRff;
        SoundRff = result.SoundRff;

        IsEdukeConsSelected = result.Dependencies?.RequiredFeatures is not null && result.Dependencies.RequiredFeatures.Contains(FeatureEnum.EDuke32_CON);
        IsModernTypesSelected = result.Dependencies?.RequiredFeatures is not null && result.Dependencies.RequiredFeatures.Contains(FeatureEnum.Modern_Types);
        IsModelsSelected = result.Dependencies?.RequiredFeatures is not null && result.Dependencies.RequiredFeatures.Contains(FeatureEnum.Models);
        IsHightileSelected = result.Dependencies?.RequiredFeatures is not null && result.Dependencies.RequiredFeatures.Contains(FeatureEnum.Hightile);
        IsSlopedSelected = result.Dependencies?.RequiredFeatures is not null && result.Dependencies.RequiredFeatures.Contains(FeatureEnum.Sloped_Sprites);
        IsTrorSelected = result.Dependencies?.RequiredFeatures is not null && result.Dependencies.RequiredFeatures.Contains(FeatureEnum.TROR);
        IsCstatSelected = result.Dependencies?.RequiredFeatures is not null && result.Dependencies.RequiredFeatures.Contains(FeatureEnum.Wall_Rotate_Cstat);
        IsLightingSelected = result.Dependencies?.RequiredFeatures is not null && result.Dependencies.RequiredFeatures.Contains(FeatureEnum.Dynamic_Lighting);

        DependenciesList = result.Dependencies?.Addons is null ? [] : [.. result.Dependencies.Addons];
        IncompatibilitiesList = result.Incompatibles?.Addons is null ? [] : [.. result.Incompatibles.Addons];

        if (result.StartMap is MapFileDto mapFile)
        {
            IsFileMapTypeSelected = true;
            MapFileName = mapFile.File;
        }
        else if (result.StartMap is MapSlotDto slotFile)
        {
            IsElMapTypeSelected = true;
            MapEpisode = slotFile.Episode;
            MapLevel = slotFile.Level;
        }

        AddonDescription = result.Description;
    }

    [RelayCommand]
    private void AddDependency()
    {
        DependantAddonDto newAddon = new() { Id = "", Version = null };

        if (DependenciesList is null)
        {
            DependenciesList = [newAddon];
        }
        else
        {
            DependenciesList = DependenciesList.Add(newAddon);
        }
    }

    [RelayCommand]
    private void RemoveDependency(DependantAddonDto dependency)
    {
        DependenciesList = DependenciesList.Remove(dependency);
    }

    [RelayCommand]
    private void AddIncompatibility()
    {
        DependantAddonDto newAddon = new() { Id = "", Version = null };

        if (IncompatibilitiesList is null)
        {
            IncompatibilitiesList = [newAddon];
        }
        else
        {
            IncompatibilitiesList = IncompatibilitiesList.Add(newAddon);
        }
    }

    [RelayCommand]
    private void RemoveIncompatibility(DependantAddonDto dependency)
    {
        IncompatibilitiesList = IncompatibilitiesList.Remove(dependency);
    }

    [RelayCommand]
    private async Task SelectFileForCrcAsync()
    {
        var files = await Properties.TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Choose file",
                AllowMultiple = false
            }).ConfigureAwait(true);

        if (files.Count == 0)
        {
            return;
        }

        var source = File.ReadAllBytes(files[0].Path.LocalPath);

        var crc_table = new uint[256];
        uint crc;

        for (uint i = 0; i < 256; i++)
        {
            crc = i;

            for (uint j = 0; j < 8; j++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
            }

            crc_table[i] = crc;
        };

        crc = 0xFFFFFFFF;

        foreach (var s in source)
        {
            crc = crc_table[(crc ^ s) & 0xFF] ^ (crc >> 8);
        }

        crc ^= 0xFFFFFFFF;

        GameCrc = "0x" + crc.ToString("X");
    }

    [RelayCommand]
    private void CreateZip()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(PathToAddonFolder) &&
                !Directory.Exists(PathToAddonFolder))
            {
                ErrorText = "Choose addon folder";
                return;
            }

            var addon = CreateJson();
            var jsonStr = JsonSerializer.Serialize(addon, AddonManifestContext.Default.AddonDto);

            File.WriteAllText(Path.Combine(PathToAddonFolder!, "addon.json"), jsonStr);

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
                ThrowHelper.ArgumentOutOfRangeException(nameof(addon.AddonType));
                return;
            }

            StringBuilder version = new();

            foreach (var ch in addon.Version)
            {
                if (Path.GetInvalidPathChars().Contains(ch) ||
                    ch == '.')
                {
                    version.Append('_');
                }
                else
                {
                    version.Append(ch);
                }
            }

            var archiveName = Path.Combine(archiveSaveFolder, $"{addon.Id}_v{version}.zip");

            using (var archive = ZipArchive.Create())
            {
                archive.AddAllFromDirectory(PathToAddonFolder!);
                archive.SaveTo(archiveName, CompressionType.Deflate);
            }

            ErrorText = "Zip created successfully. Go to the game page and press Refresh.";
        }
        catch (Exception ex)
        {
            ErrorText = ex.Message;
        }
    }

    private AddonDto CreateJson()
    {
        var files = Directory.GetFiles(PathToAddonFolder!, "*", SearchOption.TopDirectoryOnly).Select(static x => Path.GetFileName(x).ToLower());
        var forbidden = files.Intersect(_forbiddenNames);

        if (forbidden.Any())
        {
            ThrowHelper.Exception($"Common files names can't be used. Rename these files: {string.Join(", ", forbidden)}");
        }

        if (IsBloodSelected)
        {
            if (files.Any(static x => x.EndsWith(".ART", StringComparison.InvariantCultureIgnoreCase)))
            {
                ThrowHelper.Exception($"Don't use ART files. Convert them to DEF.");
            }
            if (files.Any(static x => x.EndsWith(".DAT", StringComparison.InvariantCultureIgnoreCase)))
            {
                ThrowHelper.Exception($"Don't use DAT files. Convert them to DEF.");
            }
            if (files.Any(static x => x.EndsWith(".RFS", StringComparison.InvariantCultureIgnoreCase)))
            {
                ThrowHelper.Exception($"Addons with RFS files are not supported");
            }
        }
            

        ErrorText = null;

        var addonType =
              IsTcSelected ? AddonTypeEnum.TC
            : IsMapSelected ? AddonTypeEnum.Map
            : IsModSelected ? AddonTypeEnum.Mod
            : ThrowHelper.Exception<AddonTypeEnum>("Select addon type");

        var gameEnum =
              IsDukeSelected ? GameEnum.Duke3D
            : IsBloodSelected ? GameEnum.Blood
            : IsWangSelected ? GameEnum.ShadowWarrior
            : IsFurySelected ? GameEnum.Fury
            : IsRedneckSelected ? GameEnum.Redneck
            : IsRidesAgainSelected ? GameEnum.RidesAgain
            : IsSlaveSelected ? GameEnum.Exhumed
            : ThrowHelper.Exception<GameEnum>("Select game");

        DukeVersionEnum? dukeVersion =
              !IsDukeSelected ? null
            : IsDukeAtomicSelected ? DukeVersionEnum.Duke3D_Atomic
            : IsDuke13DSelected ? DukeVersionEnum.Duke3D_13D
            : IsDukeWTSelected ? DukeVersionEnum.Duke3D_WT
            : null;

        if (string.IsNullOrWhiteSpace(AddonTitle))
        {
            ThrowHelper.Exception("Select addon title");
        }
        if (string.IsNullOrWhiteSpace(AddonId))
        {
            ThrowHelper.Exception("Select addon id");
        }
        if (string.IsNullOrWhiteSpace(AddonVersion))
        {
            ThrowHelper.Exception("Select addon version");
        }

        List<FeatureEnum> features = [];

        if (IsEdukeConsSelected &&
            AreDukePropertiesAvailable)
        {
            features.Add(FeatureEnum.EDuke32_CON);
        }
        if (IsModernTypesSelected &&
            IsBloodSelected)
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

        IStartMap? startMap = null;

        if (IsMapSelected)
        {
            if (!IsElMapTypeSelected && !IsFileMapTypeSelected)
            {
                ThrowHelper.Exception("Select start map");
            }

            if (IsElMapTypeSelected)
            {
                if (MapEpisode is null)
                {
                    ThrowHelper.Exception("Select start map episode");
                }

                if (MapLevel is null)
                {
                    ThrowHelper.Exception("Select start map level");
                }

                startMap = new MapSlotDto() { Episode = MapEpisode.Value, Level = MapLevel.Value };
            }

            if (IsFileMapTypeSelected)
            {
                if (string.IsNullOrWhiteSpace(MapFileName))
                {
                    ThrowHelper.Exception("Select start map file name");
                }

                startMap = new MapFileDto() { File = MapFileName };
            }
        }

        AddonDto addon = new()
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
            Author = string.IsNullOrWhiteSpace(AddonAuthor) ? null : AddonAuthor,
            Description = string.IsNullOrWhiteSpace(AddonDescription) ? null : AddonDescription,
            MainDef = string.IsNullOrWhiteSpace(MainDef) || IsModSelected ? null : MainDef,
            AdditionalDefs = string.IsNullOrWhiteSpace(AdditionalDefs) ? null : [.. AdditionalDefs.Split(',')],
            MainCon = string.IsNullOrWhiteSpace(MainCon) || !IsMainConAvailable ? null : MainCon,
            AdditionalCons = string.IsNullOrWhiteSpace(AdditionalCons) || !AreDukePropertiesAvailable ? null : [.. AdditionalCons.Split(',')],
            Rts = Rts,
            Ini = Ini,
            MainRff = MainRff,
            SoundRff = SoundRff,
            Dependencies = (DependenciesList is null || DependenciesList.Count == 0) && features.Count == 0 ? null : new() { Addons = DependenciesList is null || DependenciesList.Count == 0 ? null : [.. DependenciesList], RequiredFeatures = features.Count == 0 ? null : features },
            Incompatibles = (IncompatibilitiesList is null || IncompatibilitiesList.Count == 0) ? null : new() { Addons = IncompatibilitiesList is null || IncompatibilitiesList.Count == 0 ? null : [.. IncompatibilitiesList] },
            StartMap = startMap
        };

        var jsonStr = JsonSerializer.Serialize(addon, AddonManifestContext.Default.AddonDto);
        JsonText = jsonStr;

        return addon;
    }

    [RelayCommand]
    private async Task UploadAddonAsync()
    {
        FilePickerFileType z64 = new("Zipped addon")
        {
            Patterns = ["*.zip"]
        };

        var files = await Properties.TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Choose addon file",
                AllowMultiple = false,
                FileTypeFilter = [z64]
            }).ConfigureAwait(true);

        if (files.Count == 0)
        {
            return;
        }

        var checkResult = _filesUploader.CheckAddonBeforeUploading(files[0].Path.LocalPath);

        if (checkResult is not null)
        {
            UploadStatus = checkResult;
            return;
        }

        var uploadResult = await _filesUploader.UploadFilesToFtpAsync(files[0].Path.LocalPath, CancellationToken.None);

        if (uploadResult)
        {
            UploadStatus = "Uploaded successfully";
        }
        else
        { 
            UploadStatus = "Error while uploading file";
        }
    }

    [RelayCommand]
    private void PreviewJson()
    {
        try
        {
            var addon = CreateJson();
        }
        catch (Exception ex)
        {
            ErrorText = ex.Message;
        }
    }

    [RelayCommand]
    private void SaveJson()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(PathToAddonFolder) &&
                !Directory.Exists(PathToAddonFolder))
            {
                ErrorText = "Choose addon folder";
                return;
            }

            var addon = CreateJson();
            var jsonStr = JsonSerializer.Serialize(addon, AddonManifestContext.Default.AddonDto);

            File.WriteAllText(Path.Combine(PathToAddonFolder!, "addon.json"), jsonStr);

            ErrorText = "JSON saved";
        }
        catch (Exception ex)
        {
            ErrorText = ex.Message;
        }
    }

    #endregion
}
