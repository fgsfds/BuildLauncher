using Avalonia.Desktop.Helpers;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using Common.Client.Tools;
using Common.Enums;
using Common.Enums.Versions;
using Common.Helpers;
using Common.Interfaces;
using Common.Serializable.Addon;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Games.Providers;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class DevViewModel : ObservableObject
{
    private readonly IConfigProvider _config;
    private readonly FilesUploader _filesUploader;
    private readonly InstalledGamesProvider _gamesProvider;
    private readonly ILogger _logger;

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


    public DevViewModel(
        IConfigProvider config,
        FilesUploader filesUploader,
        InstalledGamesProvider gamesProvider,
        ILogger logger
        )
    {
        _config = config;
        _filesUploader = filesUploader;
        _gamesProvider = gamesProvider;
        _logger = logger;

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
            OnPropertyChanged();
        }
    }

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

    public bool IsDeveloperMode => ClientProperties.IsDeveloperMode;
    public bool IsStep2Visible => IsMapSelected || IsModSelected || IsTcSelected;
    public bool IsStep3Visible => SelectedGame is not null;
    public bool AreDukePropertiesAvailable => SelectedGame is GameEnum.Duke3D or GameEnum.Fury or GameEnum.Redneck or GameEnum.RidesAgain or GameEnum.NAM or GameEnum.WW2GI;
    public bool IsMainConAvailable => AreDukePropertiesAvailable && !IsModSelected;
    public bool AreBloodPropertiesAvailable => SelectedGame is GameEnum.Blood;

    [ObservableProperty]
    private string _addonId = string.Empty;
    [ObservableProperty]
    private string _addonVersion = string.Empty;
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
    private string? _windowsExecutable;
    [ObservableProperty]
    private string? _linuxExecutable;
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
    private bool _isSndInfoSelected;
    [ObservableProperty]
    private bool _isInProgress;
    [ObservableProperty]
    private int _progressBarValue = 0;

    [ObservableProperty]
    private string _addonTitle = string.Empty;
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
    [NotifyPropertyChangedFor(nameof(WindowsExecutable))]
    [NotifyPropertyChangedFor(nameof(LinuxExecutable))]
    [ObservableProperty]
    private GameEnum? _selectedGame;

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
    private SolidColorBrush _errorTextColor = SolidColorBrush.Parse("Red");

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
    private ImmutableList<DependantAddonJsonModel>? _dependenciesList;

    [ObservableProperty]
    private ImmutableList<DependantAddonJsonModel>? _incompatibilitiesList;

    #endregion


    #region Relay Commands

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

        foreach (var file in files)
        {
            var result = await _filesUploader.AddAddonToDatabaseAsync(file.Path.LocalPath).ConfigureAwait(true);

            if (!result)
            {
                _ = errors.AppendLine($"Error while adding {file}");
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


    [RelayCommand]
    private void AddDependency()
    {
        DependantAddonJsonModel newAddon = new() { Id = "", Version = null };

        DependenciesList ??= [];

        DependenciesList = DependenciesList.Add(newAddon);
    }


    [RelayCommand]
    private void RemoveDependency(DependantAddonJsonModel dependency)
    {
        Guard.IsNotNull(DependenciesList);

        DependenciesList = DependenciesList.Remove(dependency);
    }


    [RelayCommand]
    private void AddIncompatibility()
    {
        DependantAddonJsonModel newAddon = new() { Id = "", Version = null };

        IncompatibilitiesList ??= [];

        IncompatibilitiesList = IncompatibilitiesList.Add(newAddon);
    }


    [RelayCommand]
    private void RemoveIncompatibility(DependantAddonJsonModel dependency)
    {
        Guard.IsNotNull(IncompatibilitiesList);

        IncompatibilitiesList = IncompatibilitiesList.Remove(dependency);
    }


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

    [RelayCommand]
    private Task<string?> CreateZipAsync() => CreateZipInternalAsync();


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

            var fileName = Path.GetFileName(pathToArchive);
            var uploadUrl = Consts.UploadsFolder + "/" + Guid.NewGuid() + "/" + fileName;

            await using var fileStream = File.OpenRead(pathToArchive);
            using StreamContent content = new(fileStream);

            new Task(() =>
            {
                while (fileStream.CanSeek)
                {
                    ProgressBarValue = (int)(fileStream.Position / (double)fileStream.Length * 100);

                    Thread.Sleep(50);
                }
            }).Start();

            using HttpClient httpClient = new() { Timeout = Timeout.InfiniteTimeSpan };

            using var response = await httpClient.PutAsync(uploadUrl, content, CancellationToken.None).ConfigureAwait(true);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                SetResultMessage(errorMessage, true);
                return;
            }

            using var check = await httpClient.GetAsync(uploadUrl, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None).ConfigureAwait(true);

            FileInfo fileSize = new(pathToArchive);

            if (!check.IsSuccessStatusCode ||
                check.Content.Headers.ContentLength != fileSize.Length)
            {
                SetResultMessage("Error while uploading file", true);
                return;
            }

            SetResultMessage("Uploaded successfully", false);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Uploading cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error while uploading fix");
            SetResultMessage(ex.ToString(), true);
        }
        finally
        {
            IsInProgress = false;
            ProgressBarValue = 0;
        }
    }


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

            File.WriteAllText(Path.Combine(PathToAddonFolder!, "addon.json"), jsonString);

            RenameAddonFolder(addon);

            SetResultMessage("JSON saved", false);
        }
        catch (Exception ex)
        {
            SetResultMessage(ex.Message, true);
        }
    }

    #endregion


    private AddonJsonModel GetAddonJson(out string jsonString)
    {
        if (PathToAddonFolder is null)
        {
            ThrowHelper.ThrowMissingFieldException("Choose addon folder");
        }

        var files = Directory.GetFiles(PathToAddonFolder, "*", SearchOption.TopDirectoryOnly).Select(static x => Path.GetFileName(x).ToLower());
        var forbidden = files.Intersect(_forbiddenNames);

        if (SelectedGame is not GameEnum.Standalone &&
            WindowsExecutable is null &&
            LinuxExecutable is null &&
            MainRff is null &&
            SoundRff is null)
        {
            if (forbidden.Any())
            {
                ThrowHelper.ThrowMissingFieldException($"Common file names can't be used. Rename these files: {string.Join(", ", forbidden)}");
            }

            if (SelectedGame is GameEnum.Blood &&
                string.IsNullOrWhiteSpace(MainRff) &&
                string.IsNullOrWhiteSpace(SoundRff))
            {
                if (files.Any(static x => x.EndsWith(".ART", StringComparison.InvariantCultureIgnoreCase)))
                {
                    ThrowHelper.ThrowMissingFieldException("Don't use ART files. Convert them to DEF.");
                }
                if (files.Any(static x => x.EndsWith(".DAT", StringComparison.InvariantCultureIgnoreCase)))
                {
                    ThrowHelper.ThrowMissingFieldException("Don't use DAT files. Convert them to DEF.");
                }
                if (files.Any(static x => x.EndsWith(".RFS", StringComparison.InvariantCultureIgnoreCase)))
                {
                    ThrowHelper.ThrowMissingFieldException("Addons with RFS files are not supported");
                }
            }
        }

        ErrorText = null;

        var addonType =
              IsTcSelected ? AddonTypeEnum.TC
            : IsMapSelected ? AddonTypeEnum.Map
            : IsModSelected ? AddonTypeEnum.Mod
            : ThrowHelper.ThrowArgumentOutOfRangeException<AddonTypeEnum>("Select addon type");

        var gameEnum = SelectedGame ?? ThrowHelper.ThrowArgumentOutOfRangeException<GameEnum>("Select game");

        DukeVersionEnum? dukeVersion =
              SelectedGame is not GameEnum.Duke3D ? null
              : IsDukeAtomicSelected ? DukeVersionEnum.Duke3D_Atomic
              : IsDuke13DSelected ? DukeVersionEnum.Duke3D_13D
              : IsDukeWTSelected ? DukeVersionEnum.Duke3D_WT
              : null;

        if (string.IsNullOrWhiteSpace(AddonTitle))
        {
            ThrowHelper.ThrowMissingFieldException("Select addon title");
        }
        if (string.IsNullOrWhiteSpace(AddonId))
        {
            ThrowHelper.ThrowMissingFieldException("Select addon id");
        }
        if (string.IsNullOrWhiteSpace(AddonVersion))
        {
            ThrowHelper.ThrowMissingFieldException("Select addon version");
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

        IStartMap? startMap = null;

        if (IsMapSelected)
        {
            if (!IsElMapTypeSelected && !IsFileMapTypeSelected)
            {
                ThrowHelper.ThrowMissingFieldException("Select start map");
            }

            if (IsElMapTypeSelected)
            {
                if (MapEpisode is null)
                {
                    ThrowHelper.ThrowMissingFieldException("Select start map episode");
                }

                if (MapLevel is null)
                {
                    ThrowHelper.ThrowMissingFieldException("Select start map level");
                }

                startMap = new MapSlotJsonModel() { Episode = MapEpisode.Value, Level = MapLevel.Value };
            }

            if (IsFileMapTypeSelected)
            {
                if (string.IsNullOrWhiteSpace(MapFileName))
                {
                    ThrowHelper.ThrowMissingFieldException("Select start map file name");
                }

                startMap = new MapFileJsonModel() { File = MapFileName };
            }
        }

        Dictionary<OSEnum, string> executables = [];

        if (!string.IsNullOrWhiteSpace(WindowsExecutable))
        {
            executables.Add(OSEnum.Windows, WindowsExecutable);
        }

        if (!string.IsNullOrWhiteSpace(LinuxExecutable))
        {
            executables.Add(OSEnum.Linux, LinuxExecutable);
        }

        AddonJsonModel addon = new()
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
            Rts = string.IsNullOrWhiteSpace(Rts) ? null : Rts,
            Ini = string.IsNullOrWhiteSpace(Ini) ? null : Ini,
            MainRff = string.IsNullOrWhiteSpace(MainRff) ? null : MainRff,
            SoundRff = string.IsNullOrWhiteSpace(SoundRff) ? null : SoundRff,
            Dependencies = (DependenciesList is null || DependenciesList.Count == 0) && features.Count == 0 ? null : new() { Addons = DependenciesList is null || DependenciesList.Count == 0 ? null : [.. DependenciesList], RequiredFeatures = features.Count == 0 ? null : features },
            Incompatibles = (IncompatibilitiesList is null || IncompatibilitiesList.Count == 0) ? null : new() { Addons = IncompatibilitiesList is null || IncompatibilitiesList.Count == 0 ? null : [.. IncompatibilitiesList] },
            StartMap = startMap,
            Executables = executables.Count == 0 ? null : executables
        };

        jsonString = JsonSerializer.Serialize(addon, AddonManifestContext.Default.AddonJsonModel);
        JsonText = jsonString;

        return addon;
    }

    private void LoadJson(string pathToFile)
    {
        var jsonStr = File.ReadAllText(pathToFile);

        var result = JsonSerializer.Deserialize(jsonStr, AddonManifestContext.Default.AddonJsonModel);

        if (result is null)
        {
            return;
        }

        IsTcSelected = result.AddonType is AddonTypeEnum.TC;
        IsMapSelected = result.AddonType is AddonTypeEnum.Map;
        IsModSelected = result.AddonType is AddonTypeEnum.Mod;

        SelectedGame = result.SupportedGame.Game;

        var isDukeVersion = Enum.TryParse<DukeVersionEnum>(result.SupportedGame.Version, true, out var dukeVersion);
        IsDuke13DSelected = isDukeVersion && dukeVersion is DukeVersionEnum.Duke3D_13D;
        IsDukeAtomicSelected = isDukeVersion && dukeVersion is DukeVersionEnum.Duke3D_Atomic;
        IsDukeWTSelected = isDukeVersion && dukeVersion is DukeVersionEnum.Duke3D_WT;

        GameCrc = result.SupportedGame.Crc;

        AddonTitle = result.Title;
        AddonId = string.IsNullOrEmpty(AddonIdPrefix) ? result.Id : result.Id.Replace(AddonIdPrefix, "");
        AddonVersion = result.Version;
        AddonAuthor = result.Author;
        MainDef = result.MainDef;
        AdditionalDefs = result.AdditionalDefs is null ? null : string.Join(',', result.AdditionalDefs);
        MainCon = result.MainCon;
        AdditionalCons = result.AdditionalCons is null ? null : string.Join(',', result.AdditionalCons);
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
        IsSndInfoSelected = result.Dependencies?.RequiredFeatures is not null && result.Dependencies.RequiredFeatures.Contains(FeatureEnum.SndInfo);

        DependenciesList = result.Dependencies?.Addons is null ? null : [.. result.Dependencies.Addons];
        IncompatibilitiesList = result.Incompatibles?.Addons is null ? null : [.. result.Incompatibles.Addons];

        if (result.StartMap is MapFileJsonModel mapFile)
        {
            IsFileMapTypeSelected = true;
            MapFileName = mapFile.File;
        }
        else if (result.StartMap is MapSlotJsonModel slotFile)
        {
            IsElMapTypeSelected = true;
            MapEpisode = slotFile.Episode;
            MapLevel = slotFile.Level;
        }

        AddonDescription = result.Description;

        if (result.Executables is not null)
        {
            WindowsExecutable = result.Executables.TryGetValue(OSEnum.Windows, out var winExe) ? winExe : null;
            LinuxExecutable = result.Executables.TryGetValue(OSEnum.Linux, out var linExe) ? linExe : null;
        }
        else
        {
            WindowsExecutable = null;
            LinuxExecutable = null;
        }
    }

    /// <summary>
    /// Rename addon folder to {addon_id}_v{addon_version}
    /// </summary>
    /// <param name="addon">Addon</param>
    private void RenameAddonFolder(AddonJsonModel addon)
    {
        Guard.IsNotNull(PathToAddonFolder);

        var fullName = GetAddonFullName(addon);
        var newFolderPath = Path.Combine(Path.GetDirectoryName(PathToAddonFolder)!, fullName);

        if (!PathToAddonFolder.Equals(newFolderPath))
        {
            Directory.Move(PathToAddonFolder, newFolderPath);
            PathToAddonFolder = newFolderPath;
        }
    }

    private static string GetAddonFullName(AddonJsonModel addon)
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
    /// Set result message.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="isError">Is error.</param>
    private void SetResultMessage(string message, bool isError)
    {
        ErrorTextColor = isError ? SolidColorBrush.Parse("Red") : SolidColorBrush.Parse("Green");
        ErrorText = message;
    }

    /// <summary>
    /// Create zip archive with addon files.
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

            File.WriteAllText(Path.Combine(PathToAddonFolder!, "addon.json"), jsonString);

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
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(addon.AddonType));
                return null;
            }

            var fullName = GetAddonFullName(addon);
            var pathToArchive = Path.Combine(archiveSaveFolder, fullName + ".zip");

            SetResultMessage("Making zip. Please wait.", false);

            List<FileStream> fileStreams = [];

            using (var archive = ZipArchive.Create())
            {
                using (archive.PauseEntryRebuilding())
                {
                    var files = Directory.EnumerateFiles(PathToAddonFolder, "*.*", SearchOption.AllDirectories);
                    var filesCount = files.Count();
                    var currentFile = 0;

                    foreach (var path in files)
                    {
                        var fileInfo = new FileInfo(path);
                        FileStream? fileStream = fileInfo.OpenRead();
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

                var task = new Task(() => archive.SaveTo(pathToArchive, CompressionType.Deflate));
                task.Start();
                await task.WaitAsync(CancellationToken.None).ConfigureAwait(true);

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
}
