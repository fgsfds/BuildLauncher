using Amazon.Runtime;
using Amazon.S3;
using Core.All.Serializable;
using Core.Client.Interfaces;

namespace S3;

/// <summary>
///     Factory for creating S3-related utilities.
/// </summary>
public sealed class S3UtilitiesFactory
{
    private readonly IApiInterface _api;
    private readonly IConfigProvider _config;

    /// <summary>
    ///     Cached S3 bucket name.
    /// </summary>
    private string? _bucket;

    /// <summary>
    ///     Cached S3 subfolder within the bucket.
    /// </summary>
    private string? _subFolder;

    /// <summary>
    ///     Cached S3 configuration instance.
    /// </summary>
    private AmazonS3Config? _s3Config;

    /// <summary>
    ///     Initializes a new instance of the <see cref="S3UtilitiesFactory" /> class.
    /// </summary>
    public S3UtilitiesFactory(IConfigProvider config, IApiInterface api)
    {
        _config = config;
        _api = api;
    }

    /// <summary>
    ///     Returns the subfolder within the S3 bucket.
    /// </summary>
    /// <param name="cancellationToken">
    ///     Cancellation token.
    /// </param>
    /// <returns>
    ///     The S3 subfolder.
    /// </returns>
    public async Task<string> GetSubFolderAsync(CancellationToken cancellationToken)
    {
        await EnsureSettingsAsync(cancellationToken).ConfigureAwait(false);

        return _subFolder!;
    }

    /// <summary>
    ///     Creates an instance of <see cref="S3TransferUtilityWrapper" />.
    /// </summary>
    /// <param name="cancellationToken">
    ///     Cancellation token.
    /// </param>
    /// <returns>
    ///     A new <see cref="S3TransferUtilityWrapper" /> instance.
    /// </returns>
    public async Task<S3TransferUtilityWrapper> CreateTransferUtilityAsync(CancellationToken cancellationToken)
    {
        await EnsureSettingsAsync(cancellationToken).ConfigureAwait(false);

        return new(_s3Config!, _bucket!, _config.S3SecretKey);
    }

    /// <summary>
    ///     Creates an instance of <see cref="S3MetadataProvider" />.
    /// </summary>
    /// <param name="cancellationToken">
    ///     Cancellation token.
    /// </param>
    /// <returns>
    ///     A new <see cref="S3MetadataProvider" /> instance.
    /// </returns>
    public async Task<S3MetadataProvider> CreateMetadataProviderAsync(CancellationToken cancellationToken)
    {
        await EnsureSettingsAsync(cancellationToken).ConfigureAwait(false);

        return new(_s3Config!, _bucket!);
    }

    /// <summary>
    ///     Loads and caches the S3 settings from data.json.
    /// </summary>
    /// <param name="cancellationToken">
    ///     Cancellation token.
    /// </param>
    private async Task EnsureSettingsAsync(CancellationToken cancellationToken)
    {
        if (_s3Config is not null)
        {
            return;
        }

        var data = await _api.GetDataJsonAsync(cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Failed to load data.json.");

        if (!data.TryGetValue(DataJson.S3Endpoint, out var endpoint) || string.IsNullOrWhiteSpace(endpoint)
                                                                     || !data.TryGetValue(DataJson.S3Bucket, out var bucket) || string.IsNullOrWhiteSpace(bucket)
                                                                     || !data.TryGetValue(DataJson.S3SubFolder, out var subFolder) || string.IsNullOrWhiteSpace(subFolder))
        {
            throw new InvalidOperationException("S3 settings are missing in data.json.");
        }

        _bucket = bucket;
        _subFolder = subFolder;

        _s3Config = new()
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
            RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED
        };
    }
}
