﻿using CommunityToolkit.Diagnostics;
using SharpCompress.Archives;

namespace Common.Client.Tools;

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

        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            ThrowHelper.ThrowExternalException($"Error while downloading {url}, error: {response.StatusCode}");
        }

        await using var source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        var contentLength = response.Content.Headers.ContentLength;

        FileStream file = new(tempFile, FileMode.Create, FileAccess.Write, FileShare.None);

        if (!contentLength.HasValue)
        {
            await source.CopyToAsync(file).ConfigureAwait(false);
            await file.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            var buffer = new byte[81920];
            var totalBytesRead = 0f;
            int bytesRead;

            while ((bytesRead = await source.ReadAsync(buffer).ConfigureAwait(false)) != 0)
            {
                await file.WriteAsync(buffer.AsMemory(0, bytesRead)).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress.Report(totalBytesRead / (long)contentLength * 100);
            }

            await file.DisposeAsync().ConfigureAwait(false);
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

        var entryNumber = 1f;

        await Task.Run(() =>
        {
            using var archive = ArchiveFactory.Open(pathToArchive);
            using var reader = archive.ExtractAllEntries();

            var count = archive.Entries.Count();

            while (reader.MoveToNextEntry())
            {
                var fullName = Path.Combine(unpackTo, reader.Entry.Key!);

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

                    using var writableStream = File.OpenWrite(fullName);
                    reader.WriteEntryTo(writableStream);
                }

                progress.Report(entryNumber / count * 100);

                entryNumber++;
            }
        }).ConfigureAwait(false);
    }
}
