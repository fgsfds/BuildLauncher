using Core.Client.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace S3;

public static class DiHelper
{
    /// <summary>
    ///     Adds dependencies to work with S3.
    /// </summary>
    public static IServiceCollection WithS3FilesUploader(this IServiceCollection container)
    {
        _ = container.AddSingleton<IFilesUploader, S3FilesUploader>();

        return container.AddSingleton<S3UtilitiesFactory>();
    }
}
