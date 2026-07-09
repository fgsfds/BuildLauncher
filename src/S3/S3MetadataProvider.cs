using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Core.Client.Helpers;

namespace S3;

/// <summary>
///     S3 object metadata provider.
/// </summary>
public sealed class S3MetadataProvider
{
    private readonly string _bucket;

    /// <summary>
    ///     Amazon S3 client instance.
    /// </summary>
    private readonly AmazonS3Client _client;

    /// <summary>
    ///     Initializes a new instance of the <see cref="S3MetadataProvider" /> class.
    /// </summary>
    public S3MetadataProvider(AmazonS3Config config, string bucket)
    {
        _bucket = bucket;

        _client = new(new AnonymousAWSCredentials(), config);
    }

    /// <summary>
    ///     Returns remote file metadata.
    /// </summary>
    /// <param name="fileKey">Object key.</param>
    /// <returns>Metadata.</returns>
    public async Task<RemoteFileMetadata> GetMetadata(string fileKey)
    {
        var request = new GetObjectMetadataRequest
        {
            BucketName = _bucket,
            Key = fileKey
        };

        var response = await _client.GetObjectMetadataAsync(request).ConfigureAwait(false);

        return new()
        {
            Size = response.ContentLength,
            LastModified = response.LastModified,
            Url = new($"{S3Constants.S3Endpoint}/{S3Constants.S3Bucket}/{fileKey}")
        };
    }
}
