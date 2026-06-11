using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3;

/// <summary>
/// S3 object metadata provider.
/// </summary>
public sealed class S3MetadataProvider
{
    private readonly AmazonS3Client _client;
    private readonly string _bucket;

    public S3MetadataProvider(AmazonS3Config config, string bucket)
    {
        _bucket = bucket;

        _client = new(new AnonymousAWSCredentials(), config);
    }

    /// <summary>
    /// Returns remote file metadata.
    /// </summary>
    /// <param name="objectKey">Object key.</param>
    /// <returns>Metadata.</returns>
    public async Task<S3ObjectMetadata> GetMetadata(string objectKey)
    {
        var request = new GetObjectMetadataRequest
        {
            BucketName = _bucket,
            Key = objectKey,
        };

        var response = await _client.GetObjectMetadataAsync(request).ConfigureAwait(false);

        return new()
        {
            Size = response.ContentLength,
            LastModified = response.LastModified
        };
    }
}

public readonly struct S3ObjectMetadata
{
    /// <summary>
    /// File size.
    /// </summary>
    public readonly long Size { get; init; }

    /// <summary>
    /// File modified date.
    /// </summary>
    public readonly DateTime? LastModified { get; init; }
}
