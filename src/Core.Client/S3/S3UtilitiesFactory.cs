using Amazon.Runtime;
using Amazon.S3;
using Core.All.Helpers;
using Core.Client.Interfaces;

namespace Core.Client.S3;

public sealed class S3UtilitiesFactory
{
    private readonly IConfigProvider _config;

    private static readonly AmazonS3Config _s3config = new()
    {
        ServiceURL = CommonConstants.S3Endpoint,
        ForcePathStyle = true,
        RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED
    };

    public S3UtilitiesFactory(IConfigProvider config)
    {
        _config = config;
    }

    public S3TransferUtilityWrapper CreateTransferUtility()
    {
        return new(_s3config, CommonConstants.S3Bucket, _config.S3SecretKey);
    }

    public S3MetadataProvider CreateMetadataProvider()
    {
        return new(_s3config, CommonConstants.S3Bucket);
    }
}
