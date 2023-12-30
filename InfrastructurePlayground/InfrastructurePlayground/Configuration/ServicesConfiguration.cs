using LoggingPlayground;
using Messaging.Contracts;
using Messaging.HelloMessages;
using Services.Files;

namespace InfrastructurePlayground.Configuration;

public static class ServicesConfiguration
{
    public static void AddAppServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IEventProducer<HelloMessage>, HelloMessageProducer>();
        serviceCollection.AddSingleton<IAppFileProvider, S3FileProvider>();
    }
}