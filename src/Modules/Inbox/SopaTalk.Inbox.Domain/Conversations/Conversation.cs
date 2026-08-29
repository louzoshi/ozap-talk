using SopaTalk.Inbox.Domain.Conversations.Events;
using SopaTalk.SharedKernel.Domain;
using SopaTalk.SharedKernel.Results;

namespace SopaTalk.Inbox.Domain.Conversations;

public enum ConversationStatus
{
    Open = 0,
    Pending = 1,
    Snoozed = 2,
    Closed = 3,
}

/// <summary>
/// One thread of messages with a single contact on a single channel. The consistency
/// boundary for assignment, status and internal notes.
/// </summary>
public sealed class Conversation : AggregateRoot
{
    private readonly List<InternalNote> _notes = [];

    private Conversation(Guid id, Guid tenantId, Guid channelId, Guid contactId) : base(id)
    {
        TenantId = tenantId;
        ChannelId = channelId;
        ContactId = contactId;
        Status = ConversationStatus.Open;
        OpenedAtUtc = DateTimeOffset.UtcNow;
    }

    // EF Core
    private Conversation() { }

    public Guid TenantId { get; private set; }
    public Guid ChannelId { get; private set; }
    public Guid ContactId { get; private set; }
    public ConversationStatus Status { get; private set; }
    public Guid? AssignedAgentId { get; private set; }
    public Guid? TeamId { get; private set; }
    public DateTimeOffset OpenedAtUtc { get; private set; }
    public DateTimeOffset? ClosedAtUtc { get; private set; }
    public IReadOnlyList<InternalNote> Notes => _notes.AsReadOnly();

    public static Conversation Start(Guid tenantId, Guid channelId, Guid contactId)
    {
        var conversation = new Conversation(Guid.CreateVersion7(), tenantId, channelId, contactId);
        conversation.Raise(new ConversationStarted(conversation.Id, tenantId, contactId));
        return conversation;
    }

    public Result AssignTo(Guid agentId)
    {
        if (Status == ConversationStatus.Closed)
            return Result.Failure(ConversationErrors.ClosedCannotBeAssigned);

        if (AssignedAgentId == agentId)
            return Result.Success();

        AssignedAgentId = agentId;
        Status = ConversationStatus.Open;
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
}
