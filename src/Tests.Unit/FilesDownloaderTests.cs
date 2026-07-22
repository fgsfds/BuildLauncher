using System.Net;
using System.Net.Http.Headers;
using Core.All;
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

        Assert.True(result.IsSuccess);
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

        Assert.True(result.IsSuccess);
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
            downloader.DownloadFileAsync(url, destPath, CancellationToken.None));

        Assert.Contains("NotFound", ex.Message);
    }

    [Fact]
    public async Task DownloadFileAsync_ServerError_ThrowsHttpRequestException()
    {
        using var handler = new FakeDownloadHandler(HttpStatusCode.InternalServerError);
        using var httpClient = new HttpClient(handler);
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var downloader = new FilesDownloader(httpFactoryMock.Object, NullLogger<FilesDownloader>.Instance);
        var destPath = GetTempFilePath();

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            downloader.DownloadFileAsync(new Uri("http://example.com/error.zip"), destPath, CancellationToken.None));
    }

    [Fact]
    public async Task DownloadFileAsync_Forbidden_ThrowsHttpRequestException()
    {
        using var handler = new FakeDownloadHandler(HttpStatusCode.Forbidden);
        using var httpClient = new HttpClient(handler);
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var downloader = new FilesDownloader(httpFactoryMock.Object, NullLogger<FilesDownloader>.Instance);
        var destPath = GetTempFilePath();

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            downloader.DownloadFileAsync(new Uri("http://example.com/forbidden.zip"), destPath, CancellationToken.None));
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

        Assert.Equal(ResultEnum.Cancelled, result.ResultEnum);
        Assert.False(result.IsSuccess);
        Assert.False(File.Exists(destPath));
        Assert.False(File.Exists(destPath + ".temp"));
    }

    [Fact]
    public async Task DownloadFileAsync_HttpIOException_RecoversViaContinueDownload()
    {
        var data = new byte[50_000];
        new Random(42).NextBytes(data);
        using var handler = new FaultThenSuccessHandler(data);
        using var httpClient = new HttpClient(handler);
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var downloader = new FilesDownloader(httpFactoryMock.Object, NullLogger<FilesDownloader>.Instance);
        var destPath = GetTempFilePath();
        var url = new Uri("http://example.com/file.zip");

        var result = await downloader.DownloadFileAsync(url, destPath, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(File.Exists(destPath));
        Assert.Equal(data, await File.ReadAllBytesAsync(destPath));
        Assert.True(handler.CallCount >= 2);
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

        Assert.True(result.IsSuccess);
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

    [Fact]
    public async Task DownloadFileAsync_OverwritesExistingFile()
    {
        var data = new byte[10_000];
        new Random(42).NextBytes(data);
        using var handler = new FakeDownloadHandler(HttpStatusCode.OK, data);
        using var httpClient = new HttpClient(handler);
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var destPath = GetTempFilePath();
        await File.WriteAllTextAsync(destPath, "old content");

        var downloader = new FilesDownloader(httpFactoryMock.Object, NullLogger<FilesDownloader>.Instance);
        var url = new Uri("http://example.com/file.zip");

        var result = await downloader.DownloadFileAsync(url, destPath, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(data, await File.ReadAllBytesAsync(destPath));
    }

    private string GetTempFilePath() => Path.Combine(_tempDir, Guid.NewGuid().ToString());


    private sealed class FakeDownloadHandler : HttpMessageHandler
    {
        private readonly byte[] _data;
        private readonly bool _hasContentLength;
        private readonly HttpStatusCode _statusCode;

        public FakeDownloadHandler(HttpStatusCode statusCode, byte[]? data = null, bool hasContentLength = true)
        {
            _statusCode = statusCode;
            _data = data ?? [];
            _hasContentLength = hasContentLength;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
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


    private sealed class FaultThenSuccessHandler : HttpMessageHandler
    {
        private readonly byte[] _data;
        private int _callCount;
        private readonly long _bytesBeforeFault;

        public int CallCount => _callCount;

        public FaultThenSuccessHandler(byte[] data, long? bytesBeforeFault = null)
        {
            _data = data;
            _bytesBeforeFault = bytesBeforeFault ?? data.Length / 2;
            _callCount = 0;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var call = Interlocked.Increment(ref _callCount);

            if (call == 1)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new FaultyContent(_data, _bytesBeforeFault);
                response.Content.Headers.ContentLength = _data.Length;
                return Task.FromResult(response);
            }

            var range = request.Headers.Range;
            var from = range?.Ranges.FirstOrDefault()?.From ?? 0;
            var start = (int)from;
            var remaining = _data[start..];
            var response2 = new HttpResponseMessage(HttpStatusCode.PartialContent);
            response2.Content = new ByteArrayContent(remaining);
            response2.Content.Headers.ContentLength = remaining.Length;
            response2.Content.Headers.ContentRange = new ContentRangeHeaderValue(from, _data.Length - 1, _data.Length);
            return Task.FromResult(response2);
        }
    }


    private sealed class FaultyContent : ByteArrayContent
    {
        private readonly long _bytesBeforeFault;

        public FaultyContent(byte[] data, long bytesBeforeFault) : base(data)
        {
            _bytesBeforeFault = bytesBeforeFault;
        }

        protected override Stream CreateContentReadStream(CancellationToken cancellationToken)
        {
            return new FaultyReadStream(base.CreateContentReadStream(cancellationToken), _bytesBeforeFault);
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return Task.FromResult<Stream>(new FaultyReadStream(base.CreateContentReadStream(CancellationToken.None), _bytesBeforeFault));
        }
    }


    private sealed class FaultyReadStream : Stream
    {
        private readonly Stream _inner;
        private readonly long _bytesBeforeFault;
        private long _read;

        public FaultyReadStream(Stream inner, long bytesBeforeFault)
        {
            _inner = inner;
            _bytesBeforeFault = bytesBeforeFault;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => _read; set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var toRead = (int)Math.Min(count, _bytesBeforeFault - _read);
            if (toRead <= 0)
            {
                throw new HttpIOException(HttpRequestError.Unknown, "Simulated network error");
            }

            var read = _inner.Read(buffer, offset, toRead);
            _read += read;
            return read;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var toRead = (int)Math.Min(count, _bytesBeforeFault - _read);
            if (toRead <= 0)
            {
                throw new HttpIOException(HttpRequestError.Unknown, "Simulated network error");
            }

            var read = await _inner.ReadAsync(buffer, offset, toRead, cancellationToken).ConfigureAwait(false);
            _read += read;
            return read;
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
