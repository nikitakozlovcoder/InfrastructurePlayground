using Services.Options;

namespace InfrastructurePlayground.Configuration;

public static class OptionsConfiguration
{
    public static void AddAppOptions(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddOptions<S3Options>().Bind(configuration.GetSection("S3"));
    }
}