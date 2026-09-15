using OzapTalk.SharedKernel.Messaging;

namespace OzapTalk.SharedContracts.Channels;

/// <summary>A WhatsApp message arrived from a contact. Inbox turns it into a conversation + message.</summary>
public sealed record InboundMessageReceived(
    Guid TenantId,
    Guid ChannelId,
    string ContactPhone,
    string? ContactName,
    string ProviderMessageId,
    string Text,
    DateTimeOffset OccurredAtProvider) : IntegrationEvent;

/// <summary>An outbound message was accepted by WhatsApp. <paramref name="MessageId"/> is the Inbox message.</summary>
public sealed record OutboundMessageDispatched(
    Guid TenantId,
    Guid MessageId,
    string ProviderMessageId) : IntegrationEvent;

/// <summary>An outbound message could not be sent.</summary>
public sealed record OutboundMessageFailed(
    Guid TenantId,
    Guid MessageId,
    string Reason) : IntegrationEvent;
