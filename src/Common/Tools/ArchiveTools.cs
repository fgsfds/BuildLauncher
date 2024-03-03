using Common.Helpers;
using SharpCompress.Archives;

namespace Common.Tools
{
    public sealed class ArchiveTools
    {
        /// <summary>
        /// Operation progress
        /// </summary>
        public readonly Progress<float> Progress = new();

        /// <summary>
        /// Download archive
        /// </summary>
        /// <param name="url">Link to file download</param>
        /// <param name="filePath">Absolute path to destination file</param>
        /// <exception cref="Exception">Error while downloading file</exception>
        public async Task DownloadFileAsync(
            Uri url,
            string filePath)
        {
            IProgress<float> progress = Progress;
            var tempFile = filePath + ".temp";

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(10);

            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                ThrowHelper.Exception("Error while downloading a file: " + response.StatusCode.ToString());
            }

            await using var source = await response.Content.ReadAsStreamAsync();
            var contentLength = response.Content.Headers.ContentLength;

            await using FileStream file = new(tempFile, FileMode.Create, FileAccess.Write, FileShare.None);

            if (!contentLength.HasValue)
            {
                await source.CopyToAsync(file);
            }
            else
            {
                var buffer = new byte[81920];
                var totalBytesRead = 0f;
                int bytesRead;

                while ((bytesRead = await source.ReadAsync(buffer)) != 0)
                {
                    await file.WriteAsync(buffer.AsMemory(0, bytesRead));
                    totalBytesRead += bytesRead;
                    progress.Report(totalBytesRead / (long)contentLength * 100);
                }

                await file.DisposeAsync();

                File.Move(tempFile, filePath, true);
            }
        }

        /// <summary>
        /// Unpack archive
        /// </summary>
        /// <param name="pathToArchive">Absolute path to archive file</param>
        /// <param name="unpackTo">Directory to unpack archive to</param>
        public async Task UnpackArchiveAsync(
            string pathToArchive,
            string unpackTo)
        {
            IProgress<float> progress = Progress;

            using var archive = ArchiveFactory.Open(pathToArchive);
            using var reader = archive.ExtractAllEntries();

            var count = archive.Entries.Count();

            var entryNumber = 1f;

            await Task.Run(() =>
            {
                while (reader.MoveToNextEntry())
                {
                    var fullName = Path.Combine(unpackTo, reader.Entry.Key);

                    if (reader.Entry.IsDirectory)
                    {
                        Directory.CreateDirectory(fullName);
                    }
                    else
                    {
                        var directory = Path.GetDirectoryName(fullName);

                        if (directory is not null &&
                            !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        using FileStream writableStream = File.OpenWrite(fullName);
                        reader.WriteEntryTo(writableStream);
                    }

                    progress.Report(entryNumber / count * 100);

                    entryNumber++;
                }
            });
        }
    }
}
