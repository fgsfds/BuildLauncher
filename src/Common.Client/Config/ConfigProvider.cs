using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Common.All;
using Common.Client.Enums;
using Common.Client.Interfaces;
using Database.Client;
using Microsoft.EntityFrameworkCore;
using static Common.Client.Interfaces.IConfigProvider;

namespace Common.Client.Config;

public sealed class ConfigProvider : IConfigProvider
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public event ParameterChanged? ParameterChangedEvent;


    //SETTINGS
    public ThemeEnum Theme
    {
        get
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return Enum.TryParse<ThemeEnum>(dbContext.Settings.Find(nameof(Theme))?.Value, out var result) ? result : ThemeEnum.System;
        }

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

    public string? ApiPassword
    {
        get
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.Settings.Find(nameof(ApiPassword))?.Value ?? string.Empty;
        }

        set => SetSettingsValue(value ?? string.Empty);
    }

    public bool IsConsented
    {
        get => GetBoolValue(nameof(IsConsented));
        set => SetSettingsValue(value.ToString());
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

    public string? GitHubToken
    {
        get
        {
            var str = GetStringValue(nameof(GitHubToken));

            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            return Unprotect(str);
        }

        set
        {
            if (value is null)
            {
                SetSettingsValue(string.Empty);
                return;
            }

            var pro = Protect(value);
            SetSettingsValue(pro);
        }
    }

    public Dictionary<string, byte> Rating
    {
        get
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.Rating.ToDictionary(x => x.AddonId, x => x.Rating);
        }
    }

    public Dictionary<string, TimeSpan> Playtimes
    {
        get
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.Playtimes.ToDictionary(x => x.AddonId, x => x.Playtime);
        }
    }

    public HashSet<string> DisabledAutoloadMods
    {
        get
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return [.. dbContext.DisabledAddons.Select(x => x.AddonId)];
        }
    }

    public HashSet<AddonId> FavoriteAddons
    {
        get
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return [.. dbContext.Favorites.Select(x => new AddonId(x.AddonId, x.Version.Equals(string.Empty) ? null : x.Version))];
        }
    }

    public HashSet<string> GetEnabledOptions(string addonId)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var existing = dbContext.Options.FirstOrDefault(x => x.AddonId.Equals(addonId));

        if (existing is null)
        {
            return [];
        }

        return [.. existing.EnabledOptions.Split(';')];
    }

    public ConfigProvider(IDbContextFactory<DatabaseContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }


    public void AddScore(string addonId, byte rating)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var existing = dbContext.Rating.Find([addonId]);

        if (existing is null)
        {
            _ = dbContext.Rating.Add(new() { AddonId = addonId, Rating = rating });
        }
        else
        {
            existing.Rating = rating;
        }

        _ = dbContext.SaveChanges();
        ParameterChangedEvent?.Invoke(nameof(Rating));
    }

    public void AddPlaytime(string addonId, TimeSpan playTime)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var existing = dbContext.Playtimes.Find([addonId]);

        if (existing is null)
        {
            _ = dbContext.Playtimes.Add(new() { AddonId = addonId, Playtime = playTime });
        }
        else
        {
            existing.Playtime += playTime;
        }

        _ = dbContext.SaveChanges();
        ParameterChangedEvent?.Invoke(nameof(Playtimes));
    }

    public void ChangeModState(AddonId addonId, bool isEnabled)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var existing = dbContext.DisabledAddons.Find(addonId.Id);

        if (existing is null)
        {
            if (isEnabled)
            {
                return;
            }
            else
            {
                _ = dbContext.DisabledAddons.Add(new() { AddonId = addonId.Id });
            }
        }
        else
        {
            if (isEnabled)
            {
                _ = dbContext.DisabledAddons.Remove(existing);
            }
            else
            {
                return;
            }
        }

        _ = dbContext.SaveChanges();
        ParameterChangedEvent?.Invoke(nameof(DisabledAutoloadMods));
    }

    public void ChangeFavoriteState(AddonId addonId, bool isEnabled)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        if (isEnabled)
        {
            _ = dbContext.Favorites.Add(new() { AddonId = addonId.Id, Version = addonId.Version ?? string.Empty });
        }
        else
        {
            var existing = dbContext.Favorites.Find(addonId.Id, addonId.Version ?? string.Empty);

            ArgumentNullException.ThrowIfNull(existing);

            _ = dbContext.Favorites.Remove(existing);
        }

        _ = dbContext.SaveChanges();
        ParameterChangedEvent?.Invoke(nameof(FavoriteAddons));
    }

    public void ChangeAddonOptionState(string addonId, string option, bool isEnabled)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var existing = dbContext.Options.FirstOrDefault(x => x.AddonId.Equals(addonId));

        if (isEnabled)
        {
            if (existing is not null)
            {
                var enabled = existing.EnabledOptions.Split(';').ToList();
                enabled.Add(option);

                existing.EnabledOptions = string.Join(';', enabled);
            }
            else
            {
                _ = dbContext.Options.Add(new() { AddonId = addonId, EnabledOptions = option });
            }
        }
        else
        {
            ArgumentNullException.ThrowIfNull(existing);

            var enabled = existing.EnabledOptions.Split(';').ToList();
            _ = enabled.Remove(option);

            existing.EnabledOptions = string.Join(';', enabled);
        }

        _ = dbContext.SaveChanges();
    }


    private string? GetGamePath(string propertyName)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return dbContext.GamePaths.Find(propertyName)?.Path?.TrimEnd(Path.DirectorySeparatorChar);
    }

    private bool GetBoolValue(string propertyName)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return bool.TryParse(dbContext.Settings.Find(propertyName)?.Value, out var result) && result;
    }

    private string GetStringValue(string propertyName)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return dbContext.Settings.Find(propertyName)?.Value ?? string.Empty;
    }

    private void SetSettingsValue(string value, [CallerMemberName] string caller = "")
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var setting = dbContext.Settings.Find(caller);

        if (setting is null)
        {
            _ = dbContext.Settings.Add(new() { Name = caller, Value = value });
        }
        else
        {
            setting.Value = value;
        }

        _ = dbContext.SaveChanges();
        ParameterChangedEvent?.Invoke(caller);
    }

    private void SetGamePathValue(string? value, [CallerMemberName] string caller = "")
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var setting = dbContext.GamePaths.Find(caller);

        value = value?.TrimEnd(Path.DirectorySeparatorChar);

        if (string.IsNullOrWhiteSpace(value))
        {
            value = null;
        }

        if (setting is null)
        {
            _ = dbContext.GamePaths.Add(new() { Game = caller, Path = value });
        }
        else
        {
            setting.Path = value;
        }

        _ = dbContext.SaveChanges();
        ParameterChangedEvent?.Invoke(caller);
    }


    private static string Protect(string plainText)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new InvalidOperationException();
        }

        var data = Encoding.UTF8.GetBytes(plainText);
        var pro = ProtectedData.Protect(data, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(pro);
    }

    private static string? Unprotect(string cipherText)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new InvalidOperationException();
        }

        try
        {
            var bytes = Convert.FromBase64String(cipherText);
            var data = ProtectedData.Unprotect(bytes, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(data);
        }
        catch
        {
            return null;
        }
    }
}
