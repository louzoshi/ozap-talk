using Microsoft.AspNetCore.SignalR;
using SopaTalk.SharedContracts.Inbox;
using SopaTalk.SharedKernel.Messaging;

namespace SopaTalk.Api.Realtime;

/// <summary>Relays Inbox's <see cref="ConversationChanged"/> event to the tenant's SignalR group.</summary>
public sealed class ConversationChangedRelay(IHubContext<InboxHub> hub)
    : IIntegrationEventHandler<ConversationChanged>
{
    public Task HandleAsync(ConversationChanged e, CancellationToken ct = default) =>
        hub.Clients
            .Group(InboxHub.TenantGroup(e.TenantId))
            .SendAsync("conversationChanged", new { conversationId = e.ConversationId }, ct);
}
