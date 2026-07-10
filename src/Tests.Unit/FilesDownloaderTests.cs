using System.Net;
using Core.Client.Tools;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Tests.Unit;

public sealed class FilesDownloaderTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private bool _disposed;

    public FilesDownloaderTests()
    {
        _ = Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;

            try
            {
                Directory.Delete(_tempDir, true);
            }
            catch
            {
                /* ignore */
            }
        }
    }

    [Fact]
    public async Task DownloadFileAsync_Success_ReturnsTrue()
    {
        var data = new byte[200_000];
        new Random(42).NextBytes(data);
        using var handler = new FakeDownloadHandler(HttpStatusCode.OK, data);
        using var httpClient = new HttpClient(handler);
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var downloader = new FilesDownloader(httpFactoryMock.Object, NullLogger<FilesDownloader>.Instance);
        var destPath = GetTempFilePath();
        var url = new Uri("http://example.com/file.zip");

        var result = await downloader.DownloadFileAsync(url, destPath, CancellationToken.None);

        Assert.True(result);
        Assert.True(File.Exists(destPath));
        Assert.Equal(data, await File.ReadAllBytesAsync(destPath));
    }

    [Fact]
    public async Task DownloadFileAsync_WithoutContentLength_StillSucceeds()
    {
        var data = new byte[50_000];
        new Random(42).NextBytes(data);
        using var handler = new FakeDownloadHandler(HttpStatusCode.OK, data, hasContentLength: false);
        using var httpClient = new HttpClient(handler);
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var downloader = new FilesDownloader(httpFactoryMock.Object, NullLogger<FilesDownloader>.Instance);
        var destPath = GetTempFilePath();
        var url = new Uri("http://example.com/file.zip");

        var result = await downloader.DownloadFileAsync(url, destPath, CancellationToken.None);

        Assert.True(result);
        Assert.True(File.Exists(destPath));
        Assert.Equal(data, await File.ReadAllBytesAsync(destPath));
    }

    [Fact]
    public async Task DownloadFileAsync_HttpError_ThrowsHttpRequestException()
    {
        using var handler = new FakeDownloadHandler(HttpStatusCode.NotFound);
        using var httpClient = new HttpClient(handler);
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var downloader = new FilesDownloader(httpFactoryMock.Object, NullLogger<FilesDownloader>.Instance);
        var destPath = GetTempFilePath();
        var url = new Uri("http://example.com/missing.zip");

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
                                                                    downloader.DownloadFileAsync(url, destPath, CancellationToken.None)
            );

        Assert.Contains("NotFound", ex.Message);
    }

    [Fact]
    public async Task DownloadFileAsync_Cancelled_ReturnsFalseAndDeletesTemp()
    {
        var data = new byte[200_000];
        new Random(42).NextBytes(data);
        using var cts = new CancellationTokenSource();
        using var handler = new FakeDownloadHandler(HttpStatusCode.OK, data);
        using var httpClient = new HttpClient(handler);
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var downloader = new FilesDownloader(httpFactoryMock.Object, NullLogger<FilesDownloader>.Instance);
        var destPath = GetTempFilePath();
        var url = new Uri("http://example.com/file.zip");

        cts.Cancel();
        var result = await downloader.DownloadFileAsync(url, destPath, cts.Token);

        Assert.False(result);
        Assert.False(File.Exists(destPath));
        Assert.False(File.Exists(destPath + ".temp"));
    }

    [Fact]
    public async Task DownloadFileAsync_ExistingTempFile_DeletedBeforeDownload()
    {
        var data = new byte[1000];
        new Random(42).NextBytes(data);
        using var handler = new FakeDownloadHandler(HttpStatusCode.OK, data);
        using var httpClient = new HttpClient(handler);
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var destPath = GetTempFilePath();
        var tempPath = destPath + ".temp";
        await File.WriteAllTextAsync(tempPath, "stale data");

        var downloader = new FilesDownloader(httpFactoryMock.Object, NullLogger<FilesDownloader>.Instance);
        var url = new Uri("http://example.com/file.zip");

        var result = await downloader.DownloadFileAsync(url, destPath, CancellationToken.None);

        Assert.True(result);
        Assert.True(File.Exists(destPath));
        Assert.False(File.Exists(tempPath));
    }

    [Fact]
    public async Task DownloadFileAsync_WithContentLength_FiresProgress()
    {
        var data = new byte[200_000];
        new Random(42).NextBytes(data);
        using var handler = new FakeDownloadHandler(HttpStatusCode.OK, data);
        using var httpClient = new HttpClient(handler);
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var downloader = new FilesDownloader(httpFactoryMock.Object, NullLogger<FilesDownloader>.Instance);
        var destPath = GetTempFilePath();
        var url = new Uri("http://example.com/file.zip");
        var progressValues = new List<float>();

        downloader.ProgressChanged += (_, p) => progressValues.Add(p);

        _ = await downloader.DownloadFileAsync(url, destPath, CancellationToken.None);

        Assert.NotEmpty(progressValues);
        Assert.All(progressValues, v => Assert.InRange(v, 0f, 100f));
        Assert.Contains(progressValues, v => v > 0);
    }

    private string GetTempFilePath() => Path.Combine(_tempDir, Guid.NewGuid().ToString());


    private sealed class FakeDownloadHandler : HttpMessageHandler
    {
        private readonly byte[] _data;
        private readonly bool _hasContentLength;
        private readonly HttpStatusCode _statusCode;

        public FakeDownloadHandler(
            HttpStatusCode statusCode,
            byte[]? data = null,
            bool hasContentLength = true)
        {
            _statusCode = statusCode;
            _data = data ?? [];
            _hasContentLength = hasContentLength;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode);

            if (_data.Length > 0)
            {
                response.Content = _hasContentLength
                    ? new ByteArrayContent(_data)
                    : new NoLengthContent(_data);
            }

            return Task.FromResult(response);
        }
    }


    private sealed class NoLengthContent : ByteArrayContent
    {
        public NoLengthContent(byte[] data) : base(data) { }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;

            return false;
        }
    }
}
