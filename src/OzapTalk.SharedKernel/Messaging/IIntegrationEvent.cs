namespace OzapTalk.SharedKernel.Messaging;

/// <summary>
/// The ONLY contract a module exposes to other modules. Published to the in-process bus today,
/// swappable for RabbitMQ later without touching publishers or handlers.
/// Integration event types live in the module's *.Application "Contracts" folder and are the
/// module's public API surface — treat them as versioned.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredOnUtc { get; }
}

public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
    public DateTimeOffset OccurredOnUtc { get; init; } = DateTimeOffset.UtcNow;
}
