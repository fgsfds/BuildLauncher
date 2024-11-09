using Common.Client.Enums;
using Common.Client.Interfaces;
using Database.Client;
using System.Runtime.CompilerServices;
using static Common.Client.Interfaces.IConfigProvider;

namespace Common.Client.Config;

public sealed class ConfigProvider : IConfigProvider
{
    private readonly DatabaseContext _dbContext;

    public event ParameterChanged ParameterChangedEvent;

    public ConfigProvider(DatabaseContextFactory dbContextFactory)
    {
        _dbContext = dbContextFactory.Get();
    }


    //SETTINGS
    public ThemeEnum Theme
    {
        get => Enum.TryParse<ThemeEnum>(_dbContext.Settings.Find(nameof(Theme))?.Value, out var result) ? result : ThemeEnum.System;
        set => SetSettingsValue(value.ToString());
    }

    public bool SkipIntro
    {
        get => bool.TryParse(_dbContext.Settings.Find(nameof(SkipIntro))?.Value, out var result) && result;
        set => SetSettingsValue(value.ToString());
    }

    public bool SkipStartup
    {
        get => bool.TryParse(_dbContext.Settings.Find(nameof(SkipStartup))?.Value, out var result) && result;
        set => SetSettingsValue(value.ToString());
    }

    public bool UseLocalApi
    {
        get => bool.TryParse(_dbContext.Settings.Find(nameof(UseLocalApi))?.Value, out var result) && result;
        set => SetSettingsValue(value.ToString());
    }

    public string ApiPassword
    {
        get => _dbContext.Settings.Find(nameof(ApiPassword))?.Value ?? string.Empty;
        set => SetSettingsValue(value);
    }

    public Dictionary<string, byte> Rating
    {
        get => _dbContext.Rating.ToDictionary(x => x.AddonId, x => x.Rating);
    }

    public void AddScore(string addonId, byte rating)
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

    public Dictionary<string, TimeSpan> Playtimes
    {
        get => _dbContext.Playtimes.ToDictionary(x => x.AddonId, x => x.Playtime);
    }

    public void AddPlaytime(string addonId, TimeSpan playTime)
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

    public HashSet<string> DisabledAutoloadMods
    {
        get => [.. _dbContext.DisabledAddons.Select(x => x.AddonId)];
    }

    public void ChangeModState(AddonVersion addonId, bool isEnabled)
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


    //GAME PATHS
    public string? PathDuke3D
    {
        get => _dbContext.GamePaths.Find(nameof(PathDuke3D))?.Path?.TrimEnd(Path.DirectorySeparatorChar);
        set => SetGamePathValue(value);
    }

    public string? PathDukeWT
    {
        get => _dbContext.GamePaths.Find(nameof(PathDukeWT))?.Path?.TrimEnd(Path.DirectorySeparatorChar);
        set => SetGamePathValue(value);
    }

    public string? PathDuke64
    {
        get => _dbContext.GamePaths.Find(nameof(PathDuke64))?.Path?.TrimEnd(Path.DirectorySeparatorChar);
        set => SetGamePathValue(value);
    }

    public string? PathWang
    {
        get => _dbContext.GamePaths.Find(nameof(PathWang))?.Path?.TrimEnd(Path.DirectorySeparatorChar);
        set => SetGamePathValue(value);
    }

    public string? PathBlood
    {
        get => _dbContext.GamePaths.Find(nameof(PathBlood))?.Path?.TrimEnd(Path.DirectorySeparatorChar);
        set => SetGamePathValue(value);
    }

    public string? PathRedneck
    {
        get => _dbContext.GamePaths.Find(nameof(PathRedneck))?.Path?.TrimEnd(Path.DirectorySeparatorChar);
        set => SetGamePathValue(value);
    }

    public string? PathRidesAgain
    {
        get => _dbContext.GamePaths.Find(nameof(PathRidesAgain))?.Path?.TrimEnd(Path.DirectorySeparatorChar);
        set => SetGamePathValue(value);
    }

    public string? PathSlave
    {
        get => _dbContext.GamePaths.Find(nameof(PathSlave))?.Path?.TrimEnd(Path.DirectorySeparatorChar);
        set => SetGamePathValue(value);
    }

    public string? PathFury
    {
        get => _dbContext.GamePaths.Find(nameof(PathFury))?.Path?.TrimEnd(Path.DirectorySeparatorChar);
        set => SetGamePathValue(value);
    }

    public string? PathNam
    {
        get => _dbContext.GamePaths.Find(nameof(PathNam))?.Path?.TrimEnd(Path.DirectorySeparatorChar);
        set => SetGamePathValue(value);
    }

    public string? PathWW2GI
    {
        get => _dbContext.GamePaths.Find(nameof(PathWW2GI))?.Path?.TrimEnd(Path.DirectorySeparatorChar);
        set => SetGamePathValue(value);
    }

    public string? PathWitchaven
    {
        get => _dbContext.GamePaths.Find(nameof(PathWitchaven))?.Path?.TrimEnd(Path.DirectorySeparatorChar);
        set => SetGamePathValue(value);
    }

    public string? PathWitchaven2
    {
        get => _dbContext.GamePaths.Find(nameof(PathWitchaven2))?.Path?.TrimEnd(Path.DirectorySeparatorChar);
        set => SetGamePathValue(value);
    }

    public string? PathTekWar
    {
        get => _dbContext.GamePaths.Find(nameof(PathTekWar))?.Path?.TrimEnd(Path.DirectorySeparatorChar);
        set => SetGamePathValue(value);
    }



    private void SetSettingsValue(string value, [CallerMemberName] string caller = "")
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

    private void SetGamePathValue(string? value, [CallerMemberName] string caller = "")
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
