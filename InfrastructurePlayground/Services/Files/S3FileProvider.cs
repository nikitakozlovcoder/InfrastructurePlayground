using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Options;

namespace Services.Files;

public class S3FileProvider : IAppFileProvider
{
    private readonly AmazonS3Client _client;
    private readonly ILogger<S3FileProvider> _logger;
    private readonly S3Options _s3Options;

    public S3FileProvider(AmazonS3Client client,
        ILogger<S3FileProvider> logger, IOptions<S3Options> s3Options)
    {
        _client = client;
        _logger = logger;
        _s3Options = s3Options.Value;
    }

    public async Task Initialise()
    {
        if (await AmazonS3Util.DoesS3BucketExistV2Async(_client, _s3Options.Bucket))
        {
            return;
        }
        
        var result = await _client.PutBucketAsync(new PutBucketRequest
        {
            BucketName = _s3Options.Bucket
        });
        
        if (result is not { HttpStatusCode: HttpStatusCode.OK })
        {
            _logger.LogError("S3 exception");
            throw new Exception("S3 exception");
        }

        _logger.LogInformation("PutBucketAsync result {@Result}", result);
    }

    public async Task SaveFileAsync(Stream fileStream, string originalName, string contentType, CancellationToken ct)
    {
        _logger.LogInformation("SaveFile");
        
        var result = await _client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _s3Options.Bucket,
            Key = Guid.NewGuid() + Path.GetExtension(originalName),
            InputStream = fileStream,
            ContentType = contentType
        }, ct);

        if (result is not { HttpStatusCode: HttpStatusCode.OK })
        {
            _logger.LogError("S3 exception");
            throw new Exception("S3 exception");
        }
        
        _logger.LogInformation("PutObjectAsync result {@Result}", result);
    }

    public async Task DeleteFileAsync(string key, CancellationToken ct)
    {
        _logger.LogInformation("DeleteFile Key: {Key}", key);
        
        var result = await _client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _s3Options.Bucket,
            Key = key,
        }, ct);
        
        if (result is not { HttpStatusCode: HttpStatusCode.OK })
        {
            _logger.LogError("S3 exception");
            throw new Exception("S3 exception");
        }
        
        _logger.LogInformation("DeleteObjectAsync result {@Result}", result);
    }

    public Task<string> GetFileUrlAsync(string key, CancellationToken _)
    {
        var result = _client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _s3Options.Bucket,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(_s3Options.ExpirationMinutes),
            Verb = HttpVerb.GET,
            Protocol = _s3Options.UseHttps ? Protocol.HTTPS : Protocol.HTTP
        })!;

        return Task.FromResult(result);
    }
    
    public Task<string> GetUploadUrlAsync(string key, string contentType, CancellationToken _)
    {
        var result = _client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _s3Options.Bucket,
            Key = key,
            ContentType = contentType,
            Expires = DateTime.UtcNow.AddMinutes(_s3Options.ExpirationMinutes),
            Verb = HttpVerb.PUT,
            Protocol = _s3Options.UseHttps ? Protocol.HTTPS : Protocol.HTTP
        })!;

        return Task.FromResult(result);
    }
}