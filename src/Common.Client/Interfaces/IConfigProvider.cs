using Common.All;
using Common.Client.Enums;

namespace Common.Client.Interfaces;

public interface IConfigProvider
{
    HashSet<string> DisabledAutoloadMods { get; }
    HashSet<AddonId> FavoriteAddons { get; }
    bool IsConsented { get; set; }
    bool UseLocalApi { get; set; }
    string? PathBlood { get; set; }
    string? PathDuke3D { get; set; }
    string? PathDuke64 { get; set; }
    string? PathDukeWT { get; set; }
    string? PathFury { get; set; }
    string? PathRedneck { get; set; }
    string? PathRidesAgain { get; set; }
    string? PathSlave { get; set; }
    string? PathWang { get; set; }
    string? PathNam { get; set; }
    string? PathWW2GI { get; set; }
    string? PathWitchaven { get; set; }
    string? PathWitchaven2 { get; set; }
    string? PathTekWar { get; set; }
    string? ApiPassword { get; set; }
    string? GitHubToken { get; set; }
    Dictionary<string, TimeSpan> Playtimes { get; }
    Dictionary<string, byte> Rating { get; }
    bool SkipIntro { get; set; }
    bool SkipStartup { get; set; }
    ThemeEnum Theme { get; set; }

    delegate void ParameterChanged(string? parameterName);
    event ParameterChanged ParameterChangedEvent;

    void AddPlaytime(string addonId, TimeSpan playTime);
    void AddScore(string addonId, byte rating);
    void ChangeModState(AddonId addonVersion, bool isEnabled);
    void ChangeFavoriteState(AddonId addonVersion, bool isEnabled);
    void ChangeAddonOptionState(string addonId, string option, bool isEnabled);
    HashSet<string> GetEnabledOptions(string addonId);
}
