using System.Reflection;
using SharpCompress.Archives;

namespace Core.Client.Helpers;

/// <summary>
///     Provides utility methods for loading images from embedded resources and archives.
/// </summary>
public static class ImageHelper
{
    /// <summary>
    ///     Get Stream from file name
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="callingAss">Calling assembly</param>
    public static Stream FileNameToStream(string fileName, Assembly callingAss)
    {
        var assName = (callingAss.GetName().Name ?? string.Empty).Replace("BuildLauncher.", "");

        var resource = callingAss.GetManifestResourceStream($"{assName}.Assets.{fileName}");

        ArgumentNullException.ThrowIfNull(resource);

        return resource;
    }

    /// <summary>
    ///     Get grid cover from the archive
    /// </summary>
    /// <param name="archive">Archive</param>
    public static StreamedImage? GetCoverFromArchive(IArchive archive) => GetImageFromArchive(archive, "grid.");

    /// <summary>
    ///     Get grid cover from the archive
    /// </summary>
    /// <param name="archive">Archive</param>
    public static StreamedImage? GetPreviewFromArchive(IArchive archive) => GetImageFromArchive(archive, "preview.");

    /// <summary>
    ///     Extracts an image entry from the archive by matching the image name prefix.
    /// </summary>
    /// <param name="archive">The archive to search.</param>
    /// <param name="imageName">The image name prefix to match.</param>
    private static StreamedImage? GetImageFromArchive(IArchive archive, string imageName)
    {
        var image = archive.Entries.FirstOrDefault(x => x.Key != null && x.Key.StartsWith(imageName, StringComparison.OrdinalIgnoreCase));

        if (image is null)
        {
            return null;
        }

        var capacity = image.Size > 0 && image.Size <= int.MaxValue ? (int)image.Size : 81920;
        var memStream = new MemoryStream(capacity);

        using (var defStream = image.OpenEntryStream())
        {
            defStream.CopyTo(memStream);
        }

        memStream.Position = 0;

        return new()
        {
            Crc = image.Crc,
            Stream = memStream
        };
    }
}


/// <summary>
///     Represents a streamed image with CRC hash and an associated memory stream.
/// </summary>
public readonly struct StreamedImage : IAsyncDisposable
{
    /// <summary>
    ///     CRC hash of the image data.
    /// </summary>
    public required readonly long Crc { get; init; }

    /// <summary>
    ///     Memory stream containing the image data.
    /// </summary>
    public required readonly MemoryStream Stream { get; init; }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => Stream.DisposeAsync();
}
