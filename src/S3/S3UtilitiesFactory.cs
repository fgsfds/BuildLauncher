using Amazon.Runtime;
using Amazon.S3;
using Core.Client.Interfaces;

namespace S3;

/// <summary>
///     Factory for creating S3-related utilities.
/// </summary>
public sealed class S3UtilitiesFactory
{
    /// <summary>
    ///     Shared S3 configuration instance.
    /// </summary>
    private static readonly AmazonS3Config _s3config = new()
    {
        ServiceURL = S3Constants.S3Endpoint,
        ForcePathStyle = true,
        RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED
    };
    private readonly IConfigProvider _config;

    /// <summary>
    ///     Initializes a new instance of the <see cref="S3UtilitiesFactory" /> class.
    /// </summary>
    public S3UtilitiesFactory(IConfigProvider config)
    {
        _config = config;
    }

    /// <summary>
    ///     Creates an instance of <see cref="S3TransferUtilityWrapper" />.
    /// </summary>
    /// <returns>A new <see cref="S3TransferUtilityWrapper" /> instance.</returns>
    public S3TransferUtilityWrapper CreateTransferUtility()
    {
        return new(_s3config, S3Constants.S3Bucket, _config.S3SecretKey);
    }

    /// <summary>
    ///     Creates an instance of <see cref="S3MetadataProvider" />.
    /// </summary>
    /// <returns>A new <see cref="S3MetadataProvider" /> instance.</returns>
    public S3MetadataProvider CreateMetadataProvider()
    {
        return new(_s3config, S3Constants.S3Bucket);
    }
}
