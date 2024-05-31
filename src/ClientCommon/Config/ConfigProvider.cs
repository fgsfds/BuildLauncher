using ClientCommon.Helpers;
using Common;
using Common.Enums;
using Common.Helpers;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ClientCommon.Config;

public sealed class ConfigProvider : IConfigProvider
{
    private readonly DatabaseContext _dbContext;

    public delegate void ParameterChanged(string? parameterName);
    public event ParameterChanged ParameterChangedEvent;

    public ConfigProvider(DatabaseContextFactory dbContextFactory)
    {
        _dbContext = dbContextFactory.Get();

        ConvertOldConfig();
    }


    //SETTINGS
    public ThemeEnum Theme
    {
        get => Enum.TryParse<ThemeEnum>(_dbContext.Settings.Find([nameof(Theme)])?.Value, out var result) ? result : ThemeEnum.System;
        set => SetSettingsValue(value.ToString());
    }

    public bool SkipIntro
    {
        get => bool.TryParse(_dbContext.Settings.Find([nameof(SkipIntro)])?.Value, out var result) && result;
        set => SetSettingsValue(value.ToString());
    }

    public bool SkipStartup
    {
        get => bool.TryParse(_dbContext.Settings.Find([nameof(SkipStartup)])?.Value, out var result) && result;
        set => SetSettingsValue(value.ToString());
    }

    public bool UseLocalApi
    {
        get => bool.TryParse(_dbContext.Settings.Find([nameof(UseLocalApi)])?.Value, out var result) && result;
        set => SetSettingsValue(value.ToString());
    }

    public string ApiPassword
    {
        get => _dbContext.Settings.Find([nameof(ApiPassword)])?.Value ?? string.Empty;
        set => SetSettingsValue(value);
    }

    public Dictionary<string, bool> Scores
    {
        get => _dbContext.Scores.ToDictionary(x => x.AddonId, x => x.IsUpvoted);
    }

    public void AddScore(string addonId, bool isUpvote)
    {
        var existing = _dbContext.Scores.Find([addonId]);

        if (existing is null)
        {
            _dbContext.Scores.Add(new() { AddonId = addonId, IsUpvoted = isUpvote });
        }
        else
        {
            if (existing.IsUpvoted == isUpvote)
            {
                _dbContext.Scores.Remove(existing);
            }

            existing.IsUpvoted = isUpvote;
        }

        _dbContext.SaveChanges();
        ParameterChangedEvent?.Invoke(nameof(Scores));
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
            _dbContext.Playtimes.Add(new() { AddonId = addonId, Playtime = playTime });
        }
        else
        {
            existing.Playtime += playTime;
        }

        _dbContext.SaveChanges();
        ParameterChangedEvent?.Invoke(nameof(Playtimes));
    }

    public HashSet<string> DisabledAutoloadMods
    {
        get => [.. _dbContext.DisabledAddons.Select(x => x.AddonId)];
    }

    public void ChangeModState(AddonVersion addonId, bool isEnabled)
    {
        var existing = _dbContext.DisabledAddons.Find([addonId.Id]);

        if (existing is null)
        {
            if (isEnabled)
            {
                return;
            }
            else
            {
                _dbContext.DisabledAddons.Add(new() { AddonId = addonId.Id });
            }
        }
        else
        {
            if (isEnabled)
            {
                _dbContext.DisabledAddons.Remove(existing);
            }
            else
            {
                return;
            }
        }

        _dbContext.SaveChanges();
        ParameterChangedEvent?.Invoke(nameof(DisabledAutoloadMods));
    }


    //GAME PATHS
    public string? PathDuke3D
    {
        get => _dbContext.GamePaths.Find([nameof(PathDuke3D)])?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathDukeWT
    {
        get => _dbContext.GamePaths.Find([nameof(PathDukeWT)])?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathDuke64
    {
        get => _dbContext.GamePaths.Find([nameof(PathDuke64)])?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathWang
    {
        get => _dbContext.GamePaths.Find([nameof(PathWang)])?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathBlood
    {
        get => _dbContext.GamePaths.Find([nameof(PathBlood)])?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathRedneck
    {
        get => _dbContext.GamePaths.Find([nameof(PathRedneck)])?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathRideaAgain
    {
        get => _dbContext.GamePaths.Find([nameof(PathRideaAgain)])?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathSlave
    {
        get => _dbContext.GamePaths.Find([nameof(PathSlave)])?.Path;
        set => SetGamePathValue(value);
    }

    public string? PathFury
    {
        get => _dbContext.GamePaths.Find([nameof(PathFury)])?.Path;
        set => SetGamePathValue(value);
    }



    private void SetSettingsValue(string value, [CallerMemberName] string caller = "")
    {
        var setting = _dbContext.Settings.Find([caller]);

        if (setting is null)
        {
            _dbContext.Settings.Add(new() { Name = caller, Value = value });
        }
        else
        {
            setting.Value = value;
        }

        _dbContext.SaveChanges();
        ParameterChangedEvent?.Invoke(caller);
    }

    private void SetGamePathValue(string? value, [CallerMemberName] string caller = "")
    {
        var setting = _dbContext.GamePaths.Find([caller]);

        if (string.IsNullOrWhiteSpace(value))
        {
            value = null;
        }

        if (setting is null)
        {
            _dbContext.GamePaths.Add(new() { Game = caller, Path = value });
        }
        else
        {
            setting.Path = value;
        }

        _dbContext.SaveChanges();
        ParameterChangedEvent?.Invoke(caller);
    }



    [Obsolete]
    private void ConvertOldConfig()
    {
        if (!File.Exists(Path.Combine(ClientProperties.ExeFolderPath, "config.db")) ||
            !File.Exists(Path.Combine(ClientProperties.ExeFolderPath, Consts.ConfigFile)))
        {
            return;
        }

        FileStream fs = new(Consts.ConfigFile, FileMode.Open);
        var config = JsonSerializer.Deserialize(fs, ConfigEntityContext.Default.ConfigEntityObsolete);

        _dbContext.Settings.Add(new() { Name = nameof(config.Theme), Value = config.Theme.ToString() });
        _dbContext.Settings.Add(new() { Name = nameof(config.SkipIntro), Value = config.SkipIntro.ToString() });
        _dbContext.Settings.Add(new() { Name = nameof(config.SkipStartup), Value = config.SkipStartup.ToString() });

        _dbContext.Settings.Add(new() { Name = nameof(config.UseLocalApi), Value = config.UseLocalApi.ToString() });
        _dbContext.Settings.Add(new() { Name = nameof(config.ApiPassword), Value = config.ApiPassword });

        foreach (var addon in config.Upvotes)
        {
            _dbContext.Scores.Add(new() { AddonId = addon.Key, IsUpvoted = addon.Value });
        }

        foreach (var addon in config.DisabledAutoloadMods)
        {
            _dbContext.DisabledAddons.Add(new() { AddonId = addon });
        }

        foreach (var addon in config.Playtimes)
        {
            _dbContext.Playtimes.Add(new() { AddonId = addon.Key, Playtime = addon.Value });
        }

        _dbContext.GamePaths.Add(new() { Game = "PathDuke3D", Path = config.GamePathDuke3D });
        _dbContext.GamePaths.Add(new() { Game = "PathDukeWT", Path = config.GamePathDukeWT });
        _dbContext.GamePaths.Add(new() { Game = "PathDuke64", Path = config.GamePathDuke64 });
        _dbContext.GamePaths.Add(new() { Game = "PathWang", Path = config.GamePathWang });
        _dbContext.GamePaths.Add(new() { Game = "PathBlood", Path = config.GamePathBlood });
        _dbContext.GamePaths.Add(new() { Game = "PathRedneck", Path = config.GamePathRedneck });
        _dbContext.GamePaths.Add(new() { Game = "PathRideaAgain", Path = config.GamePathAgain });
        _dbContext.GamePaths.Add(new() { Game = "PathSlave", Path = config.GamePathSlave });
        _dbContext.GamePaths.Add(new() { Game = "PathFury", Path = config.GamePathFury });

        _dbContext.SaveChanges();

        fs.Dispose();
        File.Delete(Path.Combine(ClientProperties.ExeFolderPath, Consts.ConfigFile));
    }
}
