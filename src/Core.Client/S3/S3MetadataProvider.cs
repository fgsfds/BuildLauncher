using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace Core.Client.S3;

public sealed class S3MetadataProvider
{
    private readonly AmazonS3Client _client;
    private readonly string _bucket;

    public S3MetadataProvider(AmazonS3Config config, string bucket)
    {
        _bucket = bucket;

        _client = new(new AnonymousAWSCredentials(), config);
    }

    public async Task<GetObjectMetadataResponse> GetMetadata(string objectKey)
    {
        var request = new GetObjectMetadataRequest
        {
            BucketName = _bucket,
            Key = objectKey,
        };

        var response = await _client.GetObjectMetadataAsync(request).ConfigureAwait(false);

        return response;

        var sizeInBytes = response.ContentLength;
        var lastModified = response.LastModified;
    }
}
