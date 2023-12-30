using KafkaFlow;
using KafkaFlow.Serializer;
using Messaging.Contracts;
using Messaging.HelloMessages;

namespace InfrastructurePlayground.Configuration;

public static class KafkaConfiguration
{
    public static void AddAppKafka(this IServiceCollection serviceCollection, ConfigurationManager configuration)
    {
        var brokers = configuration.GetSection("Kafka:Brokers").Get<string[]>();
        
        serviceCollection.AddKafka(
            kafka => kafka
                .UseConsoleLog()
                .AddCluster(
                    cluster => cluster
                        .WithBrokers(brokers)
                        .CreateTopicIfNotExists(nameof(HelloMessage), 1, 1)
                        .AddProducer<HelloMessage>(producer => producer
                            .DefaultTopic(nameof(HelloMessage))
                            .AddMiddlewares(m =>
                                m.AddSerializer<JsonCoreSerializer>()
                            )
                        )
                        .AddConsumer(consumer => consumer
                            .Topic(nameof(HelloMessage))
                            .WithGroupId(nameof(HelloMessage)+"Group")
                            .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                            .WithBufferSize(1)
                            .WithWorkersCount(4)
                            .AddMiddlewares(x =>
                                x.AddDeserializer<JsonCoreDeserializer>()
                                    .AddTypedHandlers(h => h.AddHandler<HelloMessageConsumer>()
                                        .WithHandlerLifetime(InstanceLifetime.Transient))))
                )
        );
    }
}