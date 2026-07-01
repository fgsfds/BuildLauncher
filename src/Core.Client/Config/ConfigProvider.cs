using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Core.All;
using Core.Client.Enums;
using Core.Client.Interfaces;
using Database.Client;
using Microsoft.EntityFrameworkCore;

namespace Core.Client.Config;

/// <summary>
///     Provides application configuration backed by the local database.
/// </summary>
public sealed class ConfigProvider : IConfigProvider
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigProvider" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    public ConfigProvider(IDbContextFactory<DatabaseContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public event IConfigProvider.ParameterChanged? ParameterChangedEvent;


    //SETTINGS
    /// <inheritdoc />
    public ThemeEnum Theme
    {
        get
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            return Enum.TryParse<ThemeEnum>(dbContext.Settings.Find(nameof(Theme))?.Value, out var result) ? result : ThemeEnum.System;
        }

        set => SetSettingsValue(value.ToString());
    }

    /// <inheritdoc />
    public bool SkipIntro
    {
        get => GetBoolValue(nameof(SkipIntro));
        set => SetSettingsValue(value.ToString());
    }

    /// <inheritdoc />
    public bool SkipStartup
    {
        get => GetBoolValue(nameof(SkipStartup));
        set => SetSettingsValue(value.ToString());
    }

    /// <inheritdoc />
    public bool UseLocalApi
    {
        get => GetBoolValue(nameof(UseLocalApi));
        set => SetSettingsValue(value.ToString());
    }

    /// <inheritdoc />
    public string? ApiPassword
    {
        get
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            return dbContext.Settings.Find(nameof(ApiPassword))?.Value ?? string.Empty;
        }

        set => SetSettingsValue(value ?? string.Empty);
    }

    /// <inheritdoc />
    public bool IsConsented
    {
        get => GetBoolValue(nameof(IsConsented));
        set => SetSettingsValue(value.ToString());
    }


    //GAME PATHS
    /// <inheritdoc />
    public string? PathDuke3D
    {
        get => GetGamePath(nameof(PathDuke3D));
        set => SetGamePathValue(value);
    }

    /// <inheritdoc />
    public string? PathDukeWT
    {
        get => GetGamePath(nameof(PathDukeWT));
        set => SetGamePathValue(value);
    }

    /// <inheritdoc />
    public string? PathDuke64
    {
        get => GetGamePath(nameof(PathDuke64));
        set => SetGamePathValue(value);
    }

    /// <inheritdoc />
    public string? PathDukeZH
    {
        get => GetGamePath(nameof(PathDukeZH));
        set => SetGamePathValue(value);
    }

    /// <inheritdoc />
    public string? PathWang
    {
        get => GetGamePath(nameof(PathWang));
        set => SetGamePathValue(value);
    }

    /// <inheritdoc />
    public string? PathBlood
    {
        get => GetGamePath(nameof(PathBlood));
        set => SetGamePathValue(value);
    }

    /// <inheritdoc />
    public string? PathRedneck
    {
        get => GetGamePath(nameof(PathRedneck));
        set => SetGamePathValue(value);
    }

    /// <inheritdoc />
    public string? PathRidesAgain
    {
        get => GetGamePath(nameof(PathRidesAgain));
        set => SetGamePathValue(value);
    }

    /// <inheritdoc />
    public string? PathSlave
    {
        get => GetGamePath(nameof(PathSlave));
        set => SetGamePathValue(value);
    }

    /// <inheritdoc />
    public string? PathFury
    {
        get => GetGamePath(nameof(PathFury));
        set => SetGamePathValue(value);
    }

    /// <inheritdoc />
    public string? PathNam
    {
        get => GetGamePath(nameof(PathNam));
        set => SetGamePathValue(value);
    }

    /// <inheritdoc />
    public string? PathWW2GI
    {
        get => GetGamePath(nameof(PathWW2GI));
        set => SetGamePathValue(value);
    }

    /// <inheritdoc />
    public string? PathWitchaven
    {
        get => GetGamePath(nameof(PathWitchaven));
        set => SetGamePathValue(value);
    }

    /// <inheritdoc />
    public string? PathWitchaven2
    {
        get => GetGamePath(nameof(PathWitchaven2));
        set => SetGamePathValue(value);
    }

    /// <inheritdoc />
    public string? PathTekWar
    {
        get => GetGamePath(nameof(PathTekWar));
        set => SetGamePathValue(value);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public string? S3SecretKey
    {
        get
        {
            var str = GetStringValue(nameof(S3SecretKey));

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

    /// <inheritdoc />
    public Dictionary<string, byte> Rating
    {
        get
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            return dbContext.Rating.ToDictionary(x => x.AddonId, x => x.Rating);
        }
    }

    /// <inheritdoc />
    public Dictionary<string, TimeSpan> Playtimes
    {
        get
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            return dbContext.Playtimes.ToDictionary(x => x.AddonId, x => x.Playtime);
        }
    }

    /// <inheritdoc />
    public HashSet<string> DisabledAutoloadMods
    {
        get
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            return [.. dbContext.DisabledAddons.Select(x => x.AddonId)];
        }
    }

    /// <inheritdoc />
    public HashSet<AddonId> FavoriteAddons
    {
        get
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            return [.. dbContext.Favorites.Select(x => new AddonId(x.AddonId, x.Version.Equals(string.Empty) ? null : x.Version))];
        }
    }

    /// <inheritdoc />
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


    /// <inheritdoc />
    public void AddScore(string addonId, byte rating)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var existing = dbContext.Rating.Find([addonId]);

        if (existing is null)
        {
            _ = dbContext.Rating.Add(new()
            {
                AddonId = addonId,
                Rating = rating
            });
        }
        else
        {
            existing.Rating = rating;
        }

        _ = dbContext.SaveChanges();
        ParameterChangedEvent?.Invoke(nameof(Rating));
    }

    /// <inheritdoc />
    public void AddPlaytime(string addonId, TimeSpan playTime)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var existing = dbContext.Playtimes.Find([addonId]);

        if (existing is null)
        {
            _ = dbContext.Playtimes.Add(new()
            {
                AddonId = addonId,
                Playtime = playTime
            });
        }
        else
        {
            existing.Playtime += playTime;
        }

        _ = dbContext.SaveChanges();
        ParameterChangedEvent?.Invoke(nameof(Playtimes));
    }

    /// <inheritdoc />
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
                _ = dbContext.DisabledAddons.Add(new()
                {
                    AddonId = addonId.Id
                });
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

    /// <inheritdoc />
    public void ChangeFavoriteState(AddonId addonId, bool isEnabled)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var existing = dbContext.Favorites.Find(addonId.Id, addonId.Version ?? string.Empty);

        if (isEnabled)
        {
            if (existing is not null)
            {
                // already added
                return;
            }

            _ = dbContext.Favorites.Add(new()
            {
                AddonId = addonId.Id,
                Version = addonId.Version ?? string.Empty
            });
        }
        else
        {
            if (existing is null)
            {
                // already doesn't exist
                return;
            }

            _ = dbContext.Favorites.Remove(existing);
        }

        _ = dbContext.SaveChanges();
        ParameterChangedEvent?.Invoke(nameof(FavoriteAddons));
    }

    /// <inheritdoc />
    public void ChangeAddonOptionState(string addonId, string option, bool isEnabled)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var existing = dbContext.Options.FirstOrDefault(x => x.AddonId.Equals(addonId));

        if (isEnabled)
        {
            if (existing is not null)
            {
                var enabled = existing.EnabledOptions.Split(';').ToHashSet();
                enabled.Add(option);
                existing.EnabledOptions = string.Join(';', enabled);
            }
            else
            {
                _ = dbContext.Options.Add(new()
                {
                    AddonId = addonId,
                    EnabledOptions = option
                });
            }
        }
        else
        {
            if (existing is null)
            {
                // already disabled
                return;
            }

            var enabled = existing.EnabledOptions.Split(';').ToList();
            _ = enabled.Remove(option);

            existing.EnabledOptions = string.Join(';', enabled);
        }

        _ = dbContext.SaveChanges();
    }


    /// <summary>
    ///     Retrieves a game installation path from the database.
    /// </summary>
    /// <param name="propertyName">The property name identifying the game path.</param>
    /// <returns>The game path, or null if not set.</returns>
    private string? GetGamePath(string propertyName)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return dbContext.GamePaths.Find(propertyName)?.Path?.TrimEnd(Path.DirectorySeparatorChar);
    }

    /// <summary>
    ///     Retrieves a boolean setting value from the database.
    /// </summary>
    /// <param name="propertyName">The setting property name.</param>
    /// <returns>true if the value is parsed as true; otherwise, false.</returns>
    private bool GetBoolValue(string propertyName)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return bool.TryParse(dbContext.Settings.Find(propertyName)?.Value, out var result) && result;
    }

    /// <summary>
    ///     Retrieves a string setting value from the database.
    /// </summary>
    /// <param name="propertyName">The setting property name.</param>
    /// <returns>The setting value, or an empty string if not found.</returns>
    private string GetStringValue(string propertyName)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return dbContext.Settings.Find(propertyName)?.Value ?? string.Empty;
    }

    /// <summary>
    ///     Sets a setting value in the database and fires the change event.
    /// </summary>
    /// <param name="value">The value to store.</param>
    /// <param name="caller">The caller member name, automatically supplied.</param>
    private void SetSettingsValue(string value, [CallerMemberName] string caller = "")
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var setting = dbContext.Settings.Find(caller);

        if (setting is null)
        {
            _ = dbContext.Settings.Add(new()
            {
                Name = caller,
                Value = value
            });
        }
        else
        {
            setting.Value = value;
        }

        _ = dbContext.SaveChanges();
        ParameterChangedEvent?.Invoke(caller);
    }

    /// <summary>
    ///     Sets a game path value in the database and fires the change event.
    /// </summary>
    /// <param name="value">The path to store.</param>
    /// <param name="caller">The caller member name, automatically supplied.</param>
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
            _ = dbContext.GamePaths.Add(new()
            {
                Game = caller,
                Path = value
            });
        }
        else
        {
            setting.Path = value;
        }

        _ = dbContext.SaveChanges();
        ParameterChangedEvent?.Invoke(caller);
    }


    /// <summary>
    ///     Protects sensitive data using Windows Data Protection API.
    /// </summary>
    /// <param name="plainText">The plain text to protect.</param>
    /// <returns>The protected data as a base-64 encoded string.</returns>
    private static string Protect(string plainText)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new InvalidOperationException("Data protection is only supported on Windows");
        }

        var data = Encoding.UTF8.GetBytes(plainText);
        var pro = ProtectedData.Protect(data, DataProtectionScope.CurrentUser);

        return Convert.ToBase64String(pro);
    }

    /// <summary>
    ///     Unprotects sensitive data using Windows Data Protection API.
    /// </summary>
    /// <param name="cipherText">The base-64 encoded protected string.</param>
    /// <returns>The decrypted plain text, or null on failure.</returns>
    private static string? Unprotect(string cipherText)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new InvalidOperationException("Data protection is only supported on Windows");
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
