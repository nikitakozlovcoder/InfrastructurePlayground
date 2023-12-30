using InfrastructurePlayground.Controllers;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenSearch;
using Telemetry;

namespace InfrastructurePlayground.Configuration;

public static class TelemetryConfiguration
{
    public static void AddAppTelemetry(this WebApplicationBuilder builder)
    {
        Serilog.Debugging.SelfLog.Enable(Console.WriteLine);
        var openSearchUrl = builder.Configuration.GetValue<string>("Telemetry:OpenSearchUrl")!;
        var oltpCollectorUrl = builder.Configuration.GetValue<string>("Telemetry:OltpCollectorUrl")!;
        var serviceName = builder.Configuration.GetValue<string>("ServiceName")!;
        
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri(openSearchUrl))
            {
                ModifyConnectionSettings = x => x.BasicAuthentication("admin", "admin"),
                AutoRegisterTemplate = true,
                IndexFormat = $"{serviceName}-{DateTime.UtcNow:yyyy-MM}".ToLower(),
                FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
                EmitEventFailure = EmitEventFailureHandling.RaiseCallback | EmitEventFailureHandling.ThrowException | EmitEventFailureHandling.WriteToSelfLog
            })
            .WriteTo.Console());

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddSource(serviceName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(x =>
                {
                    x.Endpoint = new Uri(oltpCollectorUrl);
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddMeter(nameof(AppController))
                .AddOtlpExporter(x =>
                {
                    x.Endpoint = new Uri(oltpCollectorUrl);
                }));
        
        builder.Services.AddSingleton<ITelemetryApp>(_ => new TelemetryApp(serviceName));
    }
}