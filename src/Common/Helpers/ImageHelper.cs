using SharpCompress.Archives;
using System.Reflection;

namespace Common.Helpers
{
    public static class ImageHelper
    {
        /// <summary>
        /// Get Stream from file name
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="ass">Assembly</param>
        public static Stream FileNameToStream(string fileName, Assembly? ass = null)
        {
            if (ass is null)
            {
                ass = Assembly.GetCallingAssembly();
            }
            var resource = ass.GetManifestResourceStream($"{ass.GetName().Name!.Split('.')[1]}.Assets.{fileName}");

            resource.ThrowIfNull();

            return resource;
        }

        /// <summary>
        /// Get grid cover from the archive
        /// </summary>
        /// <param name="archive">Archive</param>
        public static Stream? GetCoverFromArchive(IArchive archive) => GetImageFromArchive(archive, "grid.");

        /// <summary>
        /// Get grid cover from the archive
        /// </summary>
        /// <param name="archive">Archive</param>
        public static Stream? GetImageFromArchive(IArchive archive, string imageName)
        {
            var image = archive.Entries.FirstOrDefault(x => x.Key.StartsWith(imageName));

            if (image is null)
            {
                return null;
            }

            using var defStream = (SharpCompress.Compressors.Deflate.DeflateStream)image.OpenEntryStream();
            using MemoryStream memStream = new();

            defStream.CopyTo(memStream);

            var buffer = memStream.GetBuffer();

            return new MemoryStream(buffer);
        }
    }
}
