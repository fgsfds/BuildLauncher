using Common;
using Common.Entities;
using Common.Enums;
using Common.Helpers;
using System.Text.Json;

namespace Web.Server.Providers
{
    public sealed class AddonsProvider
    {
        private readonly ILogger<AppReleasesProvider> _logger;
        private readonly HttpClient _httpClient;

        public GeneralReleaseEntity? AppRelease { get; private set; }

        public AddonsProvider(
            ILogger<AppReleasesProvider> logger,
            HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Return the latest new release or null if there's no newer releases
        /// </summary>
        public async Task<List<DownloadableAddonEntity>> GetAddonsListAsync(GameEnum gameEnum)
        {
            var fixesXml = await _httpClient.GetStringAsync($"{Consts.FilesRepo}/addons.json").ConfigureAwait(false);

            var addons = JsonSerializer.Deserialize(fixesXml, DownloadableAddonEntityListContext.Default.ListDownloadableAddonEntity);

            addons.ThrowIfNull();

            List<DownloadableAddonEntity> result = new();

            foreach (var addon in addons)
            {
                //hack for RR
                if (addon.Game == gameEnum ||
                    (gameEnum is GameEnum.Redneck && addon.Game is GameEnum.Redneck or GameEnum.RidesAgain))
                {
                    result.Add(addon);
                }
            }

            return result;
        }
    }
}
