﻿using System.Reflection;
using CommunityToolkit.Diagnostics;
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

        Guard.IsNotNull(resource);

        return resource;
    }

    /// <summary>
    /// Get grid cover from the archive
    /// </summary>
    /// <param name="archive">Archive</param>
    public static (long? crc, Stream? stream) GetCoverFromArchive(IArchive archive) => GetImageFromArchive(archive, "grid.");

    /// <summary>
    /// Get grid cover from the archive
    /// </summary>
    /// <param name="archive">Archive</param>
    public static (long? crc, Stream? stream) GetPreviewFromArchive(IArchive archive) => GetImageFromArchive(archive, "preview.");

    /// <summary>
    /// Get grid cover from the archive
    /// </summary>
    /// <param name="archive">Archive</param>
    /// <param name="imageName">Name of the image</param>
    private static (long? crc, Stream? stream) GetImageFromArchive(IArchive archive, string imageName)
    {
        var image = archive.Entries.FirstOrDefault(x => x.Key?.StartsWith(imageName) ?? false);

        if (image is null)
        {
            return (null, null);
        }

        using var defStream = image.OpenEntryStream();
        using MemoryStream memStream = new();

        defStream.CopyTo(memStream);

        var buffer = memStream.GetBuffer();

        return (image.Crc, new MemoryStream(buffer));
    }
}
