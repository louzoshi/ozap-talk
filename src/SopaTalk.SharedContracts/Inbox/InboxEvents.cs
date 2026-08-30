using SopaTalk.SharedKernel.Messaging;

namespace SopaTalk.SharedContracts.Inbox;

/// <summary>An agent's reply is ready to leave. Channels sends it through WhatsApp.</summary>
public sealed record OutboundMessageRequested(
    Guid TenantId,
    Guid ChannelId,
    Guid MessageId,
    string ToPhone,
    string Text) : IntegrationEvent;

/// <summary>A conversation changed (new message, assignment, status). The host relays it over SignalR.</summary>
public sealed record ConversationChanged(
    Guid TenantId,
    Guid ConversationId) : IntegrationEvent;
