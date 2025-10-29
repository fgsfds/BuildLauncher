namespace Web.Blazor.Helpers;

public sealed class S3Client : IDisposable
{
    /// <summary>
    /// Get signed URL for file uploading
    /// </summary>
    /// <param name="file">File name</param>
    /// <returns>Signed URL</returns>
    public string GetSignedUrl(string file)
    {
        return string.Empty;
    }

    public void Dispose()
    {

    }
}
