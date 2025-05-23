using System.Runtime.CompilerServices;
using Common.Client.Enums;
using Common.Client.Interfaces;
using CommunityToolkit.Diagnostics;
using Database.Client;
using static Common.Client.Interfaces.IConfigProvider;

namespace Common.Client.Config;

public sealed class ConfigProvider : IConfigProvider
{
    private readonly DatabaseContext _dbContext;
    private readonly Lock _lock = new();

    public event ParameterChanged? ParameterChangedEvent;


    //SETTINGS
    public ThemeEnum Theme
    {
        get => Enum.TryParse<ThemeEnum>(_dbContext.Settings.Find(nameof(Theme))?.Value, out var result) ? result : ThemeEnum.System;
        set => SetSettingsValue(value.ToString());
    }

    public bool SkipIntro
    {
        get => GetBoolValue(nameof(SkipIntro));
        set => SetSettingsValue(value.ToString());
    }

    public bool SkipStartup
    {
        get => GetBoolValue(nameof(SkipStartup));
        set => SetSettingsValue(value.ToString());
    }

    public bool UseLocalApi
    {
        get => GetBoolValue(nameof(UseLocalApi));
        set => SetSettingsValue(value.ToString());
    }

    public string ApiPassword
    {
        get => _dbContext.Settings.Find(nameof(ApiPassword))?.Value ?? string.Empty;
        set => SetSettingsValue(value);
    }

    //GAME PATHS
    public string? PathDuke3D
    {
        get => GetGamePath(nameof(PathDuke3D));
        set => SetGamePathValue(value);
    }

    public string? PathDukeWT
    {
        get => GetGamePath(nameof(PathDukeWT));
        set => SetGamePathValue(value);
    }

    public string? PathDuke64
    {
        get => GetGamePath(nameof(PathDuke64));
        set => SetGamePathValue(value);
    }

    public string? PathWang
    {
        get => GetGamePath(nameof(PathWang));
        set => SetGamePathValue(value);
    }

    public string? PathBlood
    {
        get => GetGamePath(nameof(PathBlood));
        set => SetGamePathValue(value);
    }

    public string? PathRedneck
    {
        get => GetGamePath(nameof(PathRedneck));
        set => SetGamePathValue(value);
    }

    public string? PathRidesAgain
    {
        get => GetGamePath(nameof(PathRidesAgain));
        set => SetGamePathValue(value);
    }

    public string? PathSlave
    {
        get => GetGamePath(nameof(PathSlave));
        set => SetGamePathValue(value);
    }

    public string? PathFury
    {
        get => GetGamePath(nameof(PathFury));
        set => SetGamePathValue(value);
    }

    public string? PathNam
    {
        get => GetGamePath(nameof(PathNam));
        set => SetGamePathValue(value);
    }

    public string? PathWW2GI
    {
        get => GetGamePath(nameof(PathWW2GI));
        set => SetGamePathValue(value);
    }

    public string? PathWitchaven
    {
        get => GetGamePath(nameof(PathWitchaven));
        set => SetGamePathValue(value);
    }

    public string? PathWitchaven2
    {
        get => GetGamePath(nameof(PathWitchaven2));
        set => SetGamePathValue(value);
    }

    public string? PathTekWar
    {
        get => GetGamePath(nameof(PathTekWar));
        set => SetGamePathValue(value);
    }

    public Dictionary<string, byte> Rating => _dbContext.Rating.ToDictionary(x => x.AddonId, x => x.Rating);

    public Dictionary<string, TimeSpan> Playtimes => _dbContext.Playtimes.ToDictionary(x => x.AddonId, x => x.Playtime);

    public HashSet<string> DisabledAutoloadMods => [.. _dbContext.DisabledAddons.Select(x => x.AddonId)];

    public HashSet<AddonId> FavoriteAddons => [.. _dbContext.Favorites.Select(x => new AddonId(x.AddonId, x.Version.Equals(string.Empty) ? null : x.Version))];

    public ConfigProvider(DatabaseContextFactory dbContextFactory)
    {
        _dbContext = dbContextFactory.Get();
    }


    public void AddScore(string addonId, byte rating)
    {
        using (_lock.EnterScope())
        {
            var existing = _dbContext.Rating.Find([addonId]);

            if (existing is null)
            {
                _ = _dbContext.Rating.Add(new() { AddonId = addonId, Rating = rating });
            }
            else
            {
                existing.Rating = rating;
            }

            _ = _dbContext.SaveChanges();
            ParameterChangedEvent?.Invoke(nameof(Rating));
        }
    }

    public void AddPlaytime(string addonId, TimeSpan playTime)
    {
        using (_lock.EnterScope())
        {
            var existing = _dbContext.Playtimes.Find([addonId]);

            if (existing is null)
            {
                _ = _dbContext.Playtimes.Add(new() { AddonId = addonId, Playtime = playTime });
            }
            else
            {
                existing.Playtime += playTime;
            }

            _ = _dbContext.SaveChanges();
            ParameterChangedEvent?.Invoke(nameof(Playtimes));
        }
    }

    public void ChangeModState(AddonId addonId, bool isEnabled)
    {
        using (_lock.EnterScope())
        {
            var existing = _dbContext.DisabledAddons.Find(addonId.Id);

            if (existing is null)
            {
                if (isEnabled)
                {
                    return;
                }
                else
                {
                    _ = _dbContext.DisabledAddons.Add(new() { AddonId = addonId.Id });
                }
            }
            else
            {
                if (isEnabled)
                {
                    _ = _dbContext.DisabledAddons.Remove(existing);
                }
                else
                {
                    return;
                }
            }

            _ = _dbContext.SaveChanges();
            ParameterChangedEvent?.Invoke(nameof(DisabledAutoloadMods));
        }
    }

    public void ChangeFavoriteState(AddonId addonVersion, bool isEnabled)
    {
        using (_lock.EnterScope())
        {
            if (isEnabled)
            {
                _ = _dbContext.Favorites.Add(new() { AddonId = addonVersion.Id, Version = addonVersion.Version ?? string.Empty });
            }
            else
            {
                var existing = _dbContext.Favorites.Find(addonVersion.Id, addonVersion.Version ?? string.Empty);

                Guard.IsNotNull(existing);

                _ = _dbContext.Favorites.Remove(existing);
            }

            _ = _dbContext.SaveChanges();
            ParameterChangedEvent?.Invoke(nameof(FavoriteAddons));
        }
    }


    private string? GetGamePath(string propertyName) => _dbContext.GamePaths.Find(propertyName)?.Path?.TrimEnd(Path.DirectorySeparatorChar);

    private bool GetBoolValue(string propertyName) => bool.TryParse(_dbContext.Settings.Find(propertyName)?.Value, out var result) && result;

    private void SetSettingsValue(string value, [CallerMemberName] string caller = "")
    {
        using (_lock.EnterScope())
        {
            var setting = _dbContext.Settings.Find(caller);

            if (setting is null)
            {
                _ = _dbContext.Settings.Add(new() { Name = caller, Value = value });
            }
            else
            {
                setting.Value = value;
            }

            _ = _dbContext.SaveChanges();
            ParameterChangedEvent?.Invoke(caller);
        }
    }

    private void SetGamePathValue(string? value, [CallerMemberName] string caller = "")
    {
        using (_lock.EnterScope())
        {
            var setting = _dbContext.GamePaths.Find(caller);

            value = value?.TrimEnd(Path.DirectorySeparatorChar);

            if (string.IsNullOrWhiteSpace(value))
            {
                value = null;
            }

            if (setting is null)
            {
                _ = _dbContext.GamePaths.Add(new() { Game = caller, Path = value });
            }
            else
            {
                setting.Path = value;
            }

            _ = _dbContext.SaveChanges();
            ParameterChangedEvent?.Invoke(caller);
        }
    }
}
