namespace Messaging.Contracts;

public class HelloMessage
{
    public Guid CorrelationId { get; }

    public HelloMessage(Guid correlationId)
    {
        CorrelationId = correlationId;
    }
}