using Common.Client.Helpers;
using Common.Enums;
using Common.Helpers;
using Database.Client;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Common.Client.Config;

public sealed class ConfigProvider : IConfigProvider
{
    private readonly DatabaseContext _dbContext;

    public delegate void ParameterChanged(string? parameterName);
    public event ParameterChanged ParameterChangedEvent;

    public ConfigProvider(DatabaseContextFactory dbContextFactory)
    {
        _dbContext = dbContextFactory.Get();

        ConvertOldConfig();

        FixTypo();
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
        get => _dbContext.GamePaths.Find(nameof(PathDuke3D))?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathDukeWT
    {
        get => _dbContext.GamePaths.Find(nameof(PathDukeWT))?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathDuke64
    {
        get => _dbContext.GamePaths.Find(nameof(PathDuke64))?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathWang
    {
        get => _dbContext.GamePaths.Find(nameof(PathWang))?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathBlood
    {
        get => _dbContext.GamePaths.Find(nameof(PathBlood))?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathRedneck
    {
        get => _dbContext.GamePaths.Find(nameof(PathRedneck))?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathRidesAgain
    {
        get => _dbContext.GamePaths.Find(nameof(PathRidesAgain))?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathSlave
    {
        get => _dbContext.GamePaths.Find(nameof(PathSlave))?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathFury
    {
        get => _dbContext.GamePaths.Find(nameof(PathFury))?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathNam
    {
        get => _dbContext.GamePaths.Find(nameof(PathNam))?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathWW2GI
    {
        get => _dbContext.GamePaths.Find(nameof(PathWW2GI))?.Path;
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



    [Obsolete]
    private void ConvertOldConfig()
    {
        if (!File.Exists(Path.Combine(ClientProperties.AppExeFolderPath, "config.db")) ||
            !File.Exists(Path.Combine(ClientProperties.AppExeFolderPath, Consts.ConfigFile)))
        {
            return;
        }

        FileStream fs = new(Consts.ConfigFile, FileMode.Open);
        var config = JsonSerializer.Deserialize(fs, ConfigEntityContext.Default.ConfigEntityObsolete);

        if (config is null)
        {
            fs.Dispose();
            return;
        }

        _ = _dbContext.Settings.Add(new() { Name = nameof(config.Theme), Value = config.Theme.ToString() });
        _ = _dbContext.Settings.Add(new() { Name = nameof(config.SkipIntro), Value = config.SkipIntro.ToString() });
        _ = _dbContext.Settings.Add(new() { Name = nameof(config.SkipStartup), Value = config.SkipStartup.ToString() });

        _ = _dbContext.Settings.Add(new() { Name = nameof(config.UseLocalApi), Value = config.UseLocalApi.ToString() });
        _ = _dbContext.Settings.Add(new() { Name = nameof(config.ApiPassword), Value = config.ApiPassword ?? string.Empty });

        
        foreach (var addon in config.DisabledAutoloadMods)
        {
            _ = _dbContext.DisabledAddons.Add(new() { AddonId = addon });
        }
        
        foreach (var addon in config.Playtimes)
        {
            _ = _dbContext.Playtimes.Add(new() { AddonId = addon.Key, Playtime = addon.Value });
        }


        _ = _dbContext.GamePaths.Add(new() { Game = "PathDuke3D", Path = config.GamePathDuke3D });
        _ = _dbContext.GamePaths.Add(new() { Game = "PathDukeWT", Path = config.GamePathDukeWT });
        _ = _dbContext.GamePaths.Add(new() { Game = "PathDuke64", Path = config.GamePathDuke64 });
        _ = _dbContext.GamePaths.Add(new() { Game = "PathWang", Path = config.GamePathWang });
        _ = _dbContext.GamePaths.Add(new() { Game = "PathBlood", Path = config.GamePathBlood });
        _ = _dbContext.GamePaths.Add(new() { Game = "PathRedneck", Path = config.GamePathRedneck });
        _ = _dbContext.GamePaths.Add(new() { Game = "PathRidesAgain", Path = config.GamePathAgain });
        _ = _dbContext.GamePaths.Add(new() { Game = "PathSlave", Path = config.GamePathSlave });
        _ = _dbContext.GamePaths.Add(new() { Game = "PathFury", Path = config.GamePathFury });

        _ = _dbContext.SaveChanges();

        fs.Dispose();
        File.Delete(Path.Combine(ClientProperties.AppExeFolderPath, Consts.ConfigFile));
    }

    [Obsolete]
    private void FixTypo()
    {
        var old = _dbContext.GamePaths.Find("PathRideaAgain");

        if (old is not null)
        {
            var game = nameof(PathRidesAgain);
            var path = old.Path;

            _ = _dbContext.GamePaths.Remove(old);
            _ = _dbContext.GamePaths.Add(new() { Game = game, Path = path });

            _ = _dbContext.SaveChanges();
        }
    }
}
