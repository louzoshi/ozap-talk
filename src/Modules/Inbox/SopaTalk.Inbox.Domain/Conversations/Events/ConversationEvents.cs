using SopaTalk.SharedKernel.Domain;

namespace SopaTalk.Inbox.Domain.Conversations.Events;

public sealed record ConversationStarted(Guid ConversationId, Guid TenantId, Guid ContactId) : IDomainEvent
{
    public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;
}

public sealed record ConversationAssigned(Guid ConversationId, Guid TenantId, Guid AgentId) : IDomainEvent
{
    public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;
}

public sealed record ConversationClosed(Guid ConversationId, Guid TenantId, Guid ContactId) : IDomainEvent
{
    public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;
}
