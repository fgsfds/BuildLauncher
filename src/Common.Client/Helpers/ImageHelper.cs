using System.Reflection;
using SharpCompress.Archives;

namespace Common.Client.Helpers;

public static class ImageHelper
{
    /// <summary>
    /// Get Stream from file name
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="callingAss">Calling assembly</param>
    public static Stream FileNameToStream(string fileName, Assembly callingAss)
    {
        var assName = callingAss.GetName().Name!.Replace("BuildLauncher.", "");

        var resource = callingAss.GetManifestResourceStream($"{assName}.Assets.{fileName}");

        ArgumentNullException.ThrowIfNull(resource);

        return resource;
    }

    /// <summary>
    /// Get grid cover from the archive
    /// </summary>
    /// <param name="archive">Archive</param>
    public static StreamedImage? GetCoverFromArchive(IArchive archive) => GetImageFromArchive(archive, "grid.");

    /// <summary>
    /// Get grid cover from the archive
    /// </summary>
    /// <param name="archive">Archive</param>
    public static StreamedImage? GetPreviewFromArchive(IArchive archive) => GetImageFromArchive(archive, "preview.");

    /// <summary>
    /// Get grid cover from the archive
    /// </summary>
    /// <param name="archive">Archive</param>
    /// <param name="imageName">Name of the image</param>
    private static StreamedImage? GetImageFromArchive(IArchive archive, string imageName)
    {
        var image = archive.Entries.FirstOrDefault(x => x.Key != null && x.Key.StartsWith(imageName, StringComparison.OrdinalIgnoreCase));

        if (image is null)
        {
            return null;
        }

        int capacity = image.Size > 0 && image.Size <= int.MaxValue ? (int)image.Size : 81920;
        var memStream = new MemoryStream(capacity);

        using (var defStream = image.OpenEntryStream())
        {
            defStream.CopyTo(memStream);
        }

        memStream.Position = 0;

        return new() { Crc = image.Crc, Stream = memStream };
    }
}

public readonly struct StreamedImage : IAsyncDisposable
{
    public required readonly long Crc { get; init; }
    public required readonly MemoryStream Stream { get; init; }

    public ValueTask DisposeAsync() => Stream.DisposeAsync();
}
