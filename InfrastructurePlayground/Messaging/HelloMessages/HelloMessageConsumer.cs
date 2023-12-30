using System.Diagnostics;
using System.Text;
using KafkaFlow;
using Messaging.Contracts;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Telemetry;

namespace Messaging.HelloMessages;

public class HelloMessageConsumer : IMessageHandler<HelloMessage>
{
    private readonly ILogger<HelloMessageConsumer> _logger;
    private readonly ITelemetryApp _telemetryApp; 

    public HelloMessageConsumer(ILogger<HelloMessageConsumer> logger, ITelemetryApp telemetryApp)
    {
        _logger = logger;
        _telemetryApp = telemetryApp;
    }

    public async Task Handle(IMessageContext context, HelloMessage message)
    {
        var ctx = Propagators.DefaultTextMapPropagator.Extract(default, context.Headers, (headers, key) =>
        {
            return headers.Where(x => x.Key == key)
                .Select(b =>
                {
                    try
                    {
                        return Encoding.UTF8.GetString(b.Value);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }).Where(x => x is not null)!;
        });

        using var activity =
            _telemetryApp.Source.StartActivity(nameof(HelloMessageConsumer), ActivityKind.Consumer, ctx.ActivityContext);
        
        Baggage.Current = ctx.Baggage;

        activity?.AddTag("correlationId", message.CorrelationId);
        activity?.SetTag("kafka.topic", context.ConsumerContext.Topic);
        activity?.SetTag("kafka.partition", context.ConsumerContext.Partition);
        activity?.SetTag("kafka.offset", context.ConsumerContext.Offset);
            
        _logger.LogInformation("HelloMessageConsumer consumed: {@Message}", message);

        var client = new HttpClient();

        var result = await client.GetAsync("https://www.google.com/");
        _logger.LogInformation("Http result: {@Result}", result);
    }
}