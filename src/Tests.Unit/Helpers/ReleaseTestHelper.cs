using System.IO.Compression;
using System.Net;
using Core.All.Enums;
using Core.All.Serializable.Downloadable;
using Moq;
using Moq.Protected;

namespace Tests.Unit.Helpers;

/// <summary> Creates isolated release archives and HTTP responses without network access. </summary>
internal static class ReleaseTestHelper
{
    /// <summary> Builds a ZIP containing the supplied text files. </summary>
    /// <param name="files">Archive paths and contents.</param>
    /// <returns>The ZIP bytes.</returns>
    public static byte[] Zip(params (string Name, string Content)[] files)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            foreach (var (name, content) in files)
            {
                using var writer = new StreamWriter(archive.CreateEntry(name).Open());
                writer.Write(content);
            }
        }

        return stream.ToArray();
    }

    /// <summary> Supplies an independent HTTP client for each request. </summary>
    /// <param name="data">The response bytes.</param>
    /// <param name="status">The response status.</param>
    /// <returns>The HTTP factory.</returns>
    public static IHttpClientFactory Http(byte[] data, HttpStatusCode status = HttpStatusCode.OK)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(() =>
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => new HttpResponseMessage(status) { Content = new ByteArrayContent(data) });
            return new HttpClient(handler.Object);
        });
        return factory.Object;
    }

    /// <summary> Creates release metadata for the fake HTTP endpoint. </summary>
    /// <param name="hash">The optional release digest.</param>
    /// <returns>The release metadata.</returns>
    public static GeneralReleaseJsonModel Release(string? hash = null) => new()
    {
        SupportedOS = OSEnum.Windows,
        Version = "2.0",
        Description = string.Empty,
        DownloadUrl = new Uri("https://example.invalid/release.zip"),
        Hash = hash
    };
}
