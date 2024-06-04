using Avalonia.Platform.Storage;
using BuildLauncher.Helpers;
using ClientCommon.Config;
using ClientCommon.Helpers;
using Common.Enums;
using Common.Enums.Versions;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mods;
using Mods.Serializable;
using Mods.Serializable.Addon;
using System.Collections.Immutable;
using System.Text;

namespace BuildLauncher.ViewModels;

public sealed partial class DevViewModel : ObservableObject
{
    private readonly IConfigProvider _config;
    private readonly FilesUploader _filesUploader;

    public DevViewModel(
        IConfigProvider config,
        FilesUploader filesUploader
        )
    {
        _config = config;
        _filesUploader = filesUploader;

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

    public string? AddonIdPrefix
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
            if (IsSlaveSelected)
            {
                return "slave-";
            }

            return null;
        }
    }

    public bool IsDevMode => ClientProperties.IsDevMode;
    public bool IsStep2Visible => IsMapSelected || IsModSelected || IsTcSelected;
    public bool IsStep3Visible => IsDukeSelected || IsBloodSelected || IsWangSelected || IsFurySelected || IsRedneckSelected || IsSlaveSelected;
    public bool AreDukePropertiesAvailable => IsDukeSelected || IsFurySelected || IsRedneckSelected;
    public bool IsMainConAvailable => AreDukePropertiesAvailable && !IsModSelected;
    public bool AreBloodPropertiesVisible => IsBloodSelected;
    public string AddonId { get; set; }
    public bool IsDukeAtomicSelected { get; set; }
    public bool IsDuke13DSelected { get; set; }
    public bool IsDukeWTSelected { get; set; }
    public string GameCrc { get; set; }
    public string AddonVersion { get; set; }
    public string AddonAuthor { get; set; }
    public string AddonDescription { get; set; }
    public string MainDef { get; set; }
    public string AdditionalDefs { get; set; }
    public string MainCon { get; set; }
    public string AdditionalCons { get; set; }
    public string Rts { get; set; }
    public string Ini { get; set; }
    public string MainRff { get; set; }
    public string SoundRff { get; set; }
    public string MapFileName { get; set; }
    public string MapEpisode { get; set; }
    public string MapLevel { get; set; }
    public bool IsEdukeConsSelected { get; set; }
    public bool IsCustomDudeSelected { get; set; }
    public bool IsModelsSelected { get; set; }
    public bool IsHightileSelected { get; set; }
    public bool IsSlopedSelected { get; set; }
    public bool IsTrorSelected { get; set; }
    public bool IsCstatSelected { get; set; }
    public bool IsLightingSelected { get; set; }

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

    public ImmutableList<DependantAddonDto> DependenciesList { get; set; }

    public ImmutableList<DependantAddonDto> IncompatibilitiesList { get; set; }

    #endregion


    #region Relay Commands

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

        OnPropertyChanged(nameof(DependenciesList));
    }

    [RelayCommand]
    private void RemoveDependency(DependantAddonDto dependency)
    {
        DependenciesList = DependenciesList.Remove(dependency);
        OnPropertyChanged(nameof(DependenciesList));
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

        OnPropertyChanged(nameof(IncompatibilitiesList));
    }

    [RelayCommand]
    private void RemoveIncompatibility(DependantAddonDto dependency)
    {
        IncompatibilitiesList = IncompatibilitiesList.Remove(dependency);
        OnPropertyChanged(nameof(IncompatibilitiesList));
    }

    [RelayCommand]
    private void CreateZip()
    {
        try
        {
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
            if (IsCustomDudeSelected &&
                IsBloodSelected)
            {
                features.Add(FeatureEnum.CustomDude);
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
                features.Add(FeatureEnum.DymanicLighting);
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
                    if (string.IsNullOrWhiteSpace(MapEpisode))
                    {
                        ThrowHelper.Exception("Select start map episode");
                    }

                    if (string.IsNullOrWhiteSpace(MapLevel))
                    {
                        ThrowHelper.Exception("Select start map level");
                    }

                    startMap = new MapSlotDto() { Episode = MapEpisode, Level = MapLevel };
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
                MainDef = string.IsNullOrWhiteSpace(MainDef) ? null : MainDef,
                AdditionalDefs = string.IsNullOrWhiteSpace(AdditionalDefs) ? null : [.. AdditionalDefs.Split(',')],
                MainCon = string.IsNullOrWhiteSpace(MainCon) || !AreDukePropertiesAvailable ? null : MainCon,
                AdditionalCons = string.IsNullOrWhiteSpace(AdditionalCons) || !AreDukePropertiesAvailable ? null : [.. AdditionalCons.Split(',')],
                Rts = Rts,
                Ini = Ini,
                MainRff = MainRff,
                SoundRff = SoundRff,
                Dependencies = (DependenciesList is null || DependenciesList.Count == 0) && features.Count == 0 ? null : new() { Addons = [.. DependenciesList], RequiredFeatures = features.Count == 0 ? null : features },
                Incompatibles = (IncompatibilitiesList is null || IncompatibilitiesList.Count == 0) ? null : new() { Addons = [.. IncompatibilitiesList] },
                StartMap = startMap
            };
        }
        catch (Exception ex)
        {
            ErrorText = ex.Message;
        }
    }

    #endregion
}
