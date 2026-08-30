using SopaTalk.Inbox.Domain.Conversations.Events;
using SopaTalk.SharedKernel.Domain;
using SopaTalk.SharedKernel.MultiTenancy;
using SopaTalk.SharedKernel.Results;

namespace SopaTalk.Inbox.Domain.Conversations;

public enum ConversationStatus
{
    /// <summary>Waiting on the team.</summary>
    Open = 0,

    /// <summary>Waiting on the contact (agent replied last).</summary>
    Waiting = 1,

    Closed = 2,
}

/// <summary>
/// One thread of messages with a single contact on a single channel. The consistency
/// boundary for assignment, status, unread count and internal notes.
/// </summary>
public sealed class Conversation : AggregateRoot, ITenantOwned
{
    private readonly List<InternalNote> _notes = [];

    private Conversation(Guid id, Guid tenantId, Guid channelId, Guid contactId) : base(id)
    {
        TenantId = tenantId;
        ChannelId = channelId;
        ContactId = contactId;
        Status = ConversationStatus.Open;
        OpenedAtUtc = DateTimeOffset.UtcNow;
        LastActivityAtUtc = OpenedAtUtc;
    }

    // EF Core
    private Conversation() { }

    public Guid TenantId { get; private set; }
    public Guid ChannelId { get; private set; }
    public Guid ContactId { get; private set; }
    public ConversationStatus Status { get; private set; }
    public Guid? AssignedAgentId { get; private set; }
    public string? LastMessagePreview { get; private set; }
    public DateTimeOffset LastActivityAtUtc { get; private set; }
    public int UnreadCount { get; private set; }
    public DateTimeOffset OpenedAtUtc { get; private set; }
    public DateTimeOffset? ClosedAtUtc { get; private set; }
    public IReadOnlyList<InternalNote> Notes => _notes.AsReadOnly();

    public static Conversation Start(Guid tenantId, Guid channelId, Guid contactId)
    {
        var conversation = new Conversation(Guid.CreateVersion7(), tenantId, channelId, contactId);
        conversation.Raise(new ConversationStarted(conversation.Id, tenantId, contactId));
        return conversation;
    }

    public void RecordInbound(string preview, DateTimeOffset at)
    {
        LastMessagePreview = Trim(preview);
        LastActivityAtUtc = at;
        UnreadCount++;
        if (Status == ConversationStatus.Closed)
            Status = ConversationStatus.Open;
        else if (Status == ConversationStatus.Waiting)
            Status = ConversationStatus.Open;
    }

    public void RecordOutbound(string preview, DateTimeOffset at)
    {
        LastMessagePreview = Trim(preview);
        LastActivityAtUtc = at;
        Status = ConversationStatus.Waiting;
    }

    public void MarkRead() => UnreadCount = 0;

    public Result AssignTo(Guid agentId)
    {
        if (Status == ConversationStatus.Closed)
            return Result.Failure(ConversationErrors.ClosedCannotBeAssigned);

        AssignedAgentId = agentId;
        Raise(new ConversationAssigned(Id, TenantId, agentId));
        return Result.Success();
    }

    public Result Close()
    {
        if (Status == ConversationStatus.Closed)
            return Result.Failure(ConversationErrors.AlreadyClosed);

        Status = ConversationStatus.Closed;
        ClosedAtUtc = DateTimeOffset.UtcNow;
        Raise(new ConversationClosed(Id, TenantId, ContactId));
        return Result.Success();
    }

    public Result Reopen()
    {
        if (Status != ConversationStatus.Closed)
            return Result.Failure(ConversationErrors.NotClosed);

        Status = ConversationStatus.Open;
        ClosedAtUtc = null;
        return Result.Success();
    }

    public InternalNote AddNote(Guid authorAgentId, string body)
    {
        var note = InternalNote.Create(Id, authorAgentId, body);
        _notes.Add(note);
        return note;
    }

    private static string Trim(string text) =>
        text.Length <= 120 ? text : text[..117] + "...";
}
