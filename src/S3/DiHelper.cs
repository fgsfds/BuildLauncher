using Core.Client.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace S3;

public static class DiHelper
{
    public static void WithS3FilesUploader(this ServiceCollection container)
    {
        _ = container.AddSingleton<IFilesUploader, S3FilesUploader>();
        _ = container.AddSingleton<S3UtilitiesFactory>();
    }
}
