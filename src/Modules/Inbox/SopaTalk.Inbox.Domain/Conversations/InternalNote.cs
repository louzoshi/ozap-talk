using SopaTalk.SharedKernel.Domain;

namespace SopaTalk.Inbox.Domain.Conversations;

/// <summary>A note visible only to the team, never sent to the contact.</summary>
public sealed class InternalNote : Entity
{
    private InternalNote(Guid id, Guid conversationId, Guid authorAgentId, string body) : base(id)
    {
        ConversationId = conversationId;
        AuthorAgentId = authorAgentId;
        Body = body;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    // EF Core
    private InternalNote() { }

    public Guid ConversationId { get; private set; }
    public Guid AuthorAgentId { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    internal static InternalNote Create(Guid conversationId, Guid authorAgentId, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("A nota interna não pode ser vazia.", nameof(body));

        return new InternalNote(Guid.CreateVersion7(), conversationId, authorAgentId, body.Trim());
    }
}
