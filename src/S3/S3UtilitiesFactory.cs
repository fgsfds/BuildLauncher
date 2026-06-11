using Amazon.Runtime;
using Amazon.S3;
using Core.Client.Interfaces;

namespace S3;

public sealed class S3UtilitiesFactory
{
    private readonly IConfigProvider _config;

    private static readonly AmazonS3Config _s3config = new()
    {
        ServiceURL = S3Constants.S3Endpoint,
        ForcePathStyle = true,
        RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED
    };

    public S3UtilitiesFactory(IConfigProvider config)
    {
        _config = config;
    }

    public S3TransferUtilityWrapper CreateTransferUtility()
    {
        return new(_s3config, S3Constants.S3Bucket, _config.S3SecretKey);
    }

    public S3MetadataProvider CreateMetadataProvider()
    {
        return new(_s3config, S3Constants.S3Bucket);
    }
}
