namespace Messaging;

public interface IEventProducer<in T>
{
    public Task ProduceAsync(T message);
}