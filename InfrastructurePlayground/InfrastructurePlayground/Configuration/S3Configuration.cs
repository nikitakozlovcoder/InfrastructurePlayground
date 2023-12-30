using Amazon.S3;
using Services.Options;

namespace InfrastructurePlayground.Configuration;

public static class S3Configuration
{
    public static void AddAppS3(this IServiceCollection serviceCollection, S3Options options)
    {
        serviceCollection.AddSingleton<AmazonS3Client>(_ => new AmazonS3Client(options.AccessKey, options.SecretKey, new AmazonS3Config
        {
            ServiceURL = options.Url,
            ForcePathStyle = true
        }));
    }
}