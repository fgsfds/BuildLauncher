using System.Diagnostics;
using Common.All.Enums;
using Common.All.Serializable.Addon;
using Common.All.Serializable.Downloadable;
using CommunityToolkit.Diagnostics;
using Database.Server;
using Database.Server.DbEntities;
using Microsoft.EntityFrameworkCore;

namespace Web.Blazor.Providers;

public sealed class DatabaseAddonsRetriever
{
    private readonly ILogger<DatabaseAddonsRetriever> _logger;
    private readonly DatabaseContextFactory _dbContextFactory;

    public GeneralReleaseJsonModel? AppRelease { get; }

    public DatabaseAddonsRetriever(
        ILogger<DatabaseAddonsRetriever> logger,
        DatabaseContextFactory dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// Return addons list for a game
    /// </summary>
    /// <param name="gameEnum">Game enum</param>
    /// <param name="dontLog">Don't log statistics</param>
    internal List<DownloadableAddonJsonModel> GetAddons(GameEnum gameEnum, bool dontLog = false)
    {
        using var dbContext = _dbContextFactory.Get();

        Stopwatch sw = new();
        sw.Start();

        Dictionary<string, AddonsDbEntity>? addons;

        if (gameEnum is GameEnum.Redneck)
        {
            addons = dbContext.Addons.AsNoTracking().Where(x => x.GameId == (byte)GameEnum.Redneck || x.GameId == (byte)GameEnum.RidesAgain).ToDictionary(static x => x.Id);
        }
        else
        {
            addons = dbContext.Addons.AsNoTracking().Where(x => x.GameId == (byte)gameEnum).ToDictionary(static x => x.Id);
        }

        var versions = dbContext.Versions.AsNoTracking().Where(x => addons.Keys.Contains(x.AddonId)).ToDictionary(static x => x.Id);
        var dependencies = dbContext.Dependencies.AsNoTracking().Where(x => versions.Keys.Contains(x.AddonVersionId)).ToLookup(static x => x.AddonVersionId);
        var installs = dbContext.Installs.AsNoTracking().ToDictionary(static x => x.AddonId, static y => y.Installs);
        var ratings = dbContext.Rating.AsNoTracking().ToDictionary(static x => x.AddonId, static y => y.Rating);

        List<DownloadableAddonJsonModel> result = new(versions.Count);

        foreach (var version in versions)
        {
            var addon = addons[version.Value.AddonId];

            List<string>? depsResult = null;
            var addonDeps = dependencies[version.Key];

            foreach (var dep in addonDeps)
            {
                depsResult ??= [];

                var depAddon = addons[dep.DependencyId];

                depsResult.Add(depAddon.Title + $"{(dep.DependencyVersion is not null ? $", {dep.DependencyVersion}" : string.Empty)}");
            }

            var hasInstalls = installs.TryGetValue(addon.Id, out var installsNumber);
            var hasRating = ratings.TryGetValue(addon.Id, out var ratingNumber);

            DownloadableAddonJsonModel newDownloadable = new()
            {
                AddonType = (AddonTypeEnum)addon.AddonType,
                Id = addon.Id,
                Game = (GameEnum)addon.GameId,
                DownloadUrl = version.Value.DownloadUrl,
                Title = addon.Title,
                Version = version.Value.Version,
                FileSize = version.Value.FileSize,
                IsDisabled = version.Value.IsDisabled,
                Description = version.Value.Description,
                Author = version.Value.Author,
                Dependencies = depsResult,
                Installs = hasInstalls ? installsNumber : 0,
                Rating = hasRating ? ratingNumber : 0,
                UpdateDate = version.Value.UpdateDate
            };

            result.Add(newDownloadable);
        }

        sw.Stop();

        if (!dontLog)
        {
            _logger.LogInformation($"Got {result.Count} addons for {gameEnum} in {sw.ElapsedMilliseconds} ms.");
        }

        return result;
    }

    internal int IncreaseNumberOfInstalls(string addonId)
    {
        using var dbContext = _dbContextFactory.Get();
        var fix = dbContext.Installs.Find(addonId);

        int newInstalls;

        if (fix is null)
        {
            InstallsDbEntity newInstallsEntity = new()
            {
                AddonId = addonId,
                Installs = 1
            };

            _ = dbContext.Installs.Add(newInstallsEntity);
            newInstalls = 1;
        }
        else
        {
            fix.Installs++;
            newInstalls = fix.Installs;
        }

        _ = dbContext.SaveChanges();
        return newInstalls;
    }

    internal decimal ChangeRating(string addonId, sbyte rating, bool isNew)
    {
        using var dbContext = _dbContextFactory.Get();
        var existingRating = dbContext.Rating.Find(addonId) ?? ThrowHelper.ThrowExternalException<RatingsDbEntity>($"Rating for {addonId} is not found");

        existingRating.RatingSum += rating;

        if (isNew)
        {
            existingRating.RatingTotal++;
        }

        _ = dbContext.SaveChanges();

        return existingRating.Rating;
    }

    internal void AddReport(string addonId, string text)
    {
        using var dbContext = _dbContextFactory.Get();

        ReportsDbEntity entity = new()
        {
            AddonId = addonId,
            ReportText = text
        };

        _ = dbContext.Reports.Add(entity);
        _ = dbContext.SaveChanges();
    }

    internal Dictionary<string, decimal> GetRating()
    {
        using var dbContext = _dbContextFactory.Get();
        return dbContext.Rating.ToDictionary(static x => x.AddonId, static y => y.Rating);
    }

    internal bool AddAddonToDatabase(AddonJsonModel addon)
    {
        using var dbContext = _dbContextFactory.Get();

        using (var transaction = dbContext.Database.BeginTransaction())
        {
            List<VersionsDbEntity>? olderVersion = null;

            var existingAddon = dbContext.Addons.Find(addon.Id);

            if (existingAddon is not null)
            {
                existingAddon.Title = addon.Title;

                olderVersion = [.. dbContext.Versions.Where(x => x.AddonId.Equals(existingAddon.Id))];
            }
            else
            {
                _ = dbContext.Addons.Add(new()
                {
                    Id = addon.Id,
                    Title = addon.Title,
                    GameId = (byte)addon.SupportedGame.Game,
                    AddonType = (byte)addon.AddonType
                });
            }

            _ = dbContext.SaveChanges();


            var existingVersion = dbContext.Versions.SingleOrDefault(x => x.AddonId == addon.Id && x.Version == addon.Version);

            if (existingVersion is not null)
            {
                return false;
            }

            _ = dbContext.Versions.Add(new()
            {
                AddonId = addon.Id,
                Version = addon.Version,
                DownloadUrl = null!, //TODO fix
                Description = addon.Description,
                IsDisabled = false,
                FileSize = 0, //TODO fix
                Author = addon.Author,
                UpdateDate = DateTime.Now.ToUniversalTime()
            });

            _ = dbContext.SaveChanges();


            if (addon.Dependencies is not null)
            {
                existingVersion = dbContext.Versions.SingleOrDefault(x => x.AddonId == addon.Id && x.Version == addon.Version);

                Guard.IsNotNull(existingVersion);

                foreach (var dep in addon.Dependencies.Addons)
                {
                    _ = dbContext.Dependencies.Add(new()
                    {
                        AddonVersionId = existingVersion.Id,
                        DependencyId = dep.Id,
                        DependencyVersion = dep.Version
                    });
                }

                _ = dbContext.SaveChanges();
            }


            var existingScore = dbContext.Rating.Find(addon.Id);

            if (existingScore is null)
            {
                _ = dbContext.Rating.Add(new()
                {
                    AddonId = addon.Id,
                    RatingSum = 0,
                    RatingTotal = 0
                });

                _ = dbContext.SaveChanges();
            }

            if (olderVersion is not null)
            {
                foreach (var version in olderVersion)
                {
                    version.IsDisabled = true;
                }

                _ = dbContext.SaveChanges();
            }

            transaction.Commit();

            return true;
        }
    }
}
