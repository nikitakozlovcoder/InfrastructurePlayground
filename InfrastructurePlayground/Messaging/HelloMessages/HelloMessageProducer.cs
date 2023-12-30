using System.Diagnostics;
using KafkaFlow;
using LoggingPlayground;
using Messaging.Contracts;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Telemetry;

namespace Messaging.HelloMessages;

public class HelloMessageProducer : IEventProducer<HelloMessage>
{
    private readonly IMessageProducer<HelloMessage> _producer;
    private readonly ILogger<HelloMessageProducer> _logger;
    private readonly ITelemetryApp _telemetryApp;

    public HelloMessageProducer(IMessageProducer<HelloMessage> producer,
        ILogger<HelloMessageProducer> logger,
        ITelemetryApp telemetryApp)
    {
        _producer = producer;
        _logger = logger;
        _telemetryApp = telemetryApp;
    }

    public Task ProduceAsync(HelloMessage message)
    {
        using var activity = _telemetryApp.Source.StartActivity(nameof(HelloMessageProducer), ActivityKind.Producer);
        activity?.SetTag("correlationId", message.CorrelationId);
        _logger.LogInformation("Producing {@Message}", message);
        
        var headers = new MessageHeaders();
        if (activity != null)
        {
            Propagators.DefaultTextMapPropagator.Inject(new PropagationContext(activity.Context, Baggage.Current), headers,
                (messageHeaders, key, value) =>
                {
                    messageHeaders.SetString(key, value);   
                });
        }
        
        return _producer
            .ProduceAsync(messageKey: message.CorrelationId.ToString(), message, headers);
    }
      
}