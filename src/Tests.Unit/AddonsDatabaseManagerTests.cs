using Core.All;
using Core.All.Enums;
using Core.All.Serializable.Addon;
using Core.All.Serializable.Downloadable;
using Core.Client.Interfaces;
using Core.Client.Tools;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Tests.Unit;

public sealed class AddonsDatabaseManagerTests
{
    [Fact]
    public async Task AddToDatabaseAsync_ApiReturnsFalse_ReturnsError()
    {
        var apiMock = new Mock<IApiInterface>();
        apiMock.Setup(x => x.AddAddonToDatabaseAsync(It.IsAny<AddonManifestJsonModel>(), It.IsAny<DownloadableAddonJsonModel>()))
            .ReturnsAsync(false);

        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "test content");

            var manifest = new AddonManifestJsonModel
            {
                Id = "test-addon",
                Title = "Test Addon",
                Version = "1.0",
                AddonType = AddonTypeEnum.TC,
                SupportedGame = new SupportedGameJsonModel { Game = GameEnum.Duke3D }
            };

            var manager = new AddonsDatabaseManager(apiMock.Object, NullLogger<AddonsDatabaseManager>.Instance);
            var result = await manager.AddToDatabaseAsync(tempFile, new Uri("http://example.com/test.zip"), manifest);

            Assert.False(result.IsSuccess);
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public async Task AddToDatabaseAsync_ApiReturnsTrue_ReturnsSuccess()
    {
        var apiMock = new Mock<IApiInterface>();
        apiMock.Setup(x => x.AddAddonToDatabaseAsync(It.IsAny<AddonManifestJsonModel>(), It.IsAny<DownloadableAddonJsonModel>()))
            .ReturnsAsync(true);

        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "test content for hash");

            var manifest = new AddonManifestJsonModel
            {
                Id = "test-addon",
                Title = "Test Addon",
                Version = "1.0",
                AddonType = AddonTypeEnum.TC,
                SupportedGame = new SupportedGameJsonModel { Game = GameEnum.Duke3D }
            };

            var manager = new AddonsDatabaseManager(apiMock.Object, NullLogger<AddonsDatabaseManager>.Instance);
            var result = await manager.AddToDatabaseAsync(tempFile, new Uri("http://example.com/test.zip"), manifest);

            Assert.True(result.IsSuccess);
            apiMock.Verify(x => x.AddAddonToDatabaseAsync(manifest, It.IsAny<DownloadableAddonJsonModel>()), Times.Once);
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }
}
