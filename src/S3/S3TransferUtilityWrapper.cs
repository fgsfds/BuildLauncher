using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace S3;

/// <summary>
/// <see cref="TransferUtility"/> wrapper that injects Referer header.
/// </summary>
public sealed class S3TransferUtilityWrapper : IDisposable
{
    private readonly AmazonS3Client _client;
    private readonly TransferUtility _transferUtility;
    private readonly string _bucket;
    private readonly string? _secretKey;

    public S3TransferUtilityWrapper(AmazonS3Config config, string bucket, string? secretKey)
    {
        _bucket = bucket;
        _secretKey = secretKey;

        _client = new(new AnonymousAWSCredentials(), config);
        _client.BeforeRequestEvent += Client_BeforeRequestEvent;

        _transferUtility = new(_client);
    }

    /// <summary>
    /// Uploads file.
    /// </summary>
    /// <param name="stream">Stream.</param>
    /// <param name="fileKey">Object key.</param>
    /// <param name="sha">File SHA256.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<TransferUtilityUploadResponse> UploadAsync(Stream stream, string fileKey, string? sha, CancellationToken cancellationToken)
    {
        var uploadRequest = new TransferUtilityUploadRequest
        {
            BucketName = _bucket,
            InputStream = stream,
            Key = fileKey,

            PartSize = 90 * 1024 * 1024,
            DisablePayloadSigning = false
        };

        if (!string.IsNullOrWhiteSpace(sha))
        {
            uploadRequest.Metadata["checksum-sha256"] = sha;
        }

        return _transferUtility.UploadWithResponseAsync(uploadRequest, cancellationToken);
    }

    private void Client_BeforeRequestEvent(object sender, RequestEventArgs e)
    {
        if (e is WebServiceRequestEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(_secretKey))
            {
                args.Headers["Referer"] = _secretKey;
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _transferUtility.Dispose();
        _client.BeforeRequestEvent -= Client_BeforeRequestEvent;
        _client.Dispose();
    }
}
