namespace SopaTalk.Inbox.Application.Queries;

public sealed record ConversationListItem(
    Guid Id,
    Guid ContactId,
    string ContactName,
    string ContactPhone,
    string? LastMessagePreview,
    DateTimeOffset LastActivityAtUtc,
    int UnreadCount,
    string Status,
    Guid? AssignedAgentId);

public sealed record MessageView(
    Guid Id,
    string Direction,
    string Status,
    string Body,
    Guid? AuthorAgentId,
    DateTimeOffset CreatedAtUtc);

public sealed record ConversationThread(
    Guid Id,
    string ContactName,
    string ContactPhone,
    string Status,
    Guid? AssignedAgentId,
    IReadOnlyList<MessageView> Messages);

/// <summary>Read-side. Implemented in Infrastructure with projections straight off the DbContext.</summary>
public interface IInboxQueries
{
    Task<IReadOnlyList<ConversationListItem>> ListConversationsAsync(string? status, CancellationToken ct);
    Task<ConversationThread?> GetThreadAsync(Guid conversationId, CancellationToken ct);
}
