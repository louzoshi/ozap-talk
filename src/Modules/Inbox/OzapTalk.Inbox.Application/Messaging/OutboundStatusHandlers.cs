using Microsoft.Extensions.Logging;
using OzapTalk.Inbox.Application.Abstractions;
using OzapTalk.SharedContracts.Channels;
using OzapTalk.SharedContracts.Inbox;
using OzapTalk.SharedKernel.Messaging;

namespace OzapTalk.Inbox.Application.Messaging;

public sealed class OutboundMessageDispatchedHandler(
    IMessageRepository messages, IInboxUnitOfWork unitOfWork, IEventBus eventBus)
    : IIntegrationEventHandler<OutboundMessageDispatched>
{
    public async Task HandleAsync(OutboundMessageDispatched e, CancellationToken ct = default)
    {
        var message = await messages.GetAsync(e.MessageId, ct);
        if (message is null)
            return;

        message.MarkSent(e.ProviderMessageId);
        await unitOfWork.SaveChangesAsync(ct);
        await eventBus.PublishAsync(new ConversationChanged(e.TenantId, message.ConversationId), ct);
    }
}

public sealed class OutboundMessageFailedHandler(
    IMessageRepository messages, IInboxUnitOfWork unitOfWork, IEventBus eventBus,
    ILogger<OutboundMessageFailedHandler> logger)
    : IIntegrationEventHandler<OutboundMessageFailed>
{
    public async Task HandleAsync(OutboundMessageFailed e, CancellationToken ct = default)
    {
        var message = await messages.GetAsync(e.MessageId, ct);
        if (message is null)
            return;

        logger.LogWarning("Outbound message {MessageId} failed: {Reason}", e.MessageId, e.Reason);
        message.MarkFailed(e.Reason);
        await unitOfWork.SaveChangesAsync(ct);
        await eventBus.PublishAsync(new ConversationChanged(e.TenantId, message.ConversationId), ct);
    }
}
