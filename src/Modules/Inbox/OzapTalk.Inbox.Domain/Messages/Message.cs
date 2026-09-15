using OzapTalk.SharedKernel.Domain;
using OzapTalk.SharedKernel.MultiTenancy;

namespace OzapTalk.Inbox.Domain.Messages;

public enum MessageDirection
{
    Inbound = 0,
    Outbound = 1,
}

public enum MessageStatus
{
    /// <summary>Inbound message we received.</summary>
    Received = 0,

    /// <summary>Outbound message waiting to be sent.</summary>
    Queued = 1,

    /// <summary>Outbound message accepted by WhatsApp.</summary>
    Sent = 2,

    /// <summary>Outbound message that failed to send.</summary>
    Failed = 3,
}

/// <summary>A single text message in a conversation. Its own aggregate so replies append
/// without loading the whole thread.</summary>
public sealed class Message : AggregateRoot, ITenantOwned
{
    private Message(
        Guid id, Guid tenantId, Guid conversationId, MessageDirection direction,
        MessageStatus status, string body, Guid? authorAgentId) : base(id)
    {
        TenantId = tenantId;
        ConversationId = conversationId;
        Direction = direction;
        Status = status;
        Body = body;
        AuthorAgentId = authorAgentId;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    // EF Core
    private Message() { }

    public Guid TenantId { get; private set; }
    public Guid ConversationId { get; private set; }
    public MessageDirection Direction { get; private set; }
    public MessageStatus Status { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public Guid? AuthorAgentId { get; private set; }
    public string? ProviderMessageId { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Message Inbound(Guid tenantId, Guid conversationId, string body, string providerMessageId)
    {
        var message = new Message(
            Guid.CreateVersion7(), tenantId, conversationId, MessageDirection.Inbound,
            MessageStatus.Received, body, authorAgentId: null);
        message.ProviderMessageId = providerMessageId;
        return message;
    }

    public static Message QueuedReply(Guid tenantId, Guid conversationId, string body, Guid authorAgentId) =>
        new(Guid.CreateVersion7(), tenantId, conversationId, MessageDirection.Outbound,
            MessageStatus.Queued, body, authorAgentId);

    public void MarkSent(string providerMessageId)
    {
        Status = MessageStatus.Sent;
        ProviderMessageId = providerMessageId;
        FailureReason = null;
    }

    public void MarkFailed(string reason)
    {
        Status = MessageStatus.Failed;
        FailureReason = reason;
    }
}
