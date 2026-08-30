using Microsoft.Extensions.DependencyInjection;

namespace SopaTalk.SharedKernel.Messaging;

/// <summary>
/// Dispatches integration events to in-process handlers, synchronously, within the
/// caller's scope and transaction. This is the seam: swapping in a broker (RabbitMQ
/// via an outbox) later changes only this class, not publishers or handlers.
/// </summary>
public sealed class InProcessEventBus(IServiceProvider services) : IEventBus
{
    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default)
        where TEvent : IIntegrationEvent
    {
        foreach (var handler in services.GetServices<IIntegrationEventHandler<TEvent>>())
        {
            await handler.HandleAsync(integrationEvent, ct);
        }
    }
}

public static class EventBusServiceCollectionExtensions
{
    public static IServiceCollection AddInProcessEventBus(this IServiceCollection services)
    {
        services.AddScoped<IEventBus, InProcessEventBus>();
        return services;
    }
}
