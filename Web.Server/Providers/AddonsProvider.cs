using Common.Entities;
using Common.Enums;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Web.Server.DbEntities;
using Web.Server.Helpers;

namespace Web.Server.Providers
{
    public sealed class AddonsProvider
    {
        private readonly ILogger<AppReleasesProvider> _logger;
        private readonly DatabaseContextFactory _dbContextFactory;

        public GeneralReleaseEntity? AppRelease { get; private set; }

        public AddonsProvider(
            ILogger<AppReleasesProvider> logger,
            DatabaseContextFactory dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// Return addons list for a game
        /// </summary>
        /// <param name="gameEnum">Game enum</param>
        public List<DownloadableAddonEntity> GetAddonsList(GameEnum gameEnum)
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
            var dependencies = dbContext.Dependencies.AsNoTracking().Where(x => versions.Keys.Contains(x.AddonVersionId)).ToLookup(x => x.AddonVersionId);

            List<DownloadableAddonEntity> result = new(versions.Count);

            foreach (var version in versions)
            {
                var addon = addons[version.Value.AddonId];

                List<string>? depsResult = null;
                var addonDeps = dependencies[version.Key];

                foreach (var dep in addonDeps)
                {
                    depsResult ??= [];

                    var depVersion = versions[dep.DependencyVersionId];
                    var depAddon = addons[depVersion.AddonId];

                    depsResult.Add(depAddon.Title);
                }

                DownloadableAddonEntity newDownloadable = new()
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
                    Dependencies = depsResult
                };

                result.Add(newDownloadable);
            }

            sw.Stop();
            _logger.LogInformation($"Got addons for {gameEnum} in {sw.ElapsedMilliseconds} ms.");

            return result;
        }
    }
}
