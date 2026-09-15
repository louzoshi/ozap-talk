namespace OzapTalk.SharedKernel.Messaging;

/// <summary>Publishes integration events. In-process implementation now; broker-backed later.</summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default)
        where TEvent : IIntegrationEvent;
}

/// <summary>Handles one integration event type. Registered per module.</summary>
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken ct = default);
}
