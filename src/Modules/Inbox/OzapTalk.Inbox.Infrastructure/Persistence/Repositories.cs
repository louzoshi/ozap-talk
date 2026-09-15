using Microsoft.EntityFrameworkCore;
using OzapTalk.Inbox.Application.Abstractions;
using OzapTalk.Inbox.Domain.Contacts;
using OzapTalk.Inbox.Domain.Conversations;
using OzapTalk.Inbox.Domain.Messages;

namespace OzapTalk.Inbox.Infrastructure.Persistence;

internal sealed class ContactRepository(InboxDbContext db) : IContactRepository
{
    public Task<Contact?> FindByPhoneAsync(string phone, CancellationToken ct) =>
        db.Contacts.FirstOrDefaultAsync(c => c.Phone == phone, ct);

    public Task<Contact?> GetAsync(Guid contactId, CancellationToken ct) =>
        db.Contacts.FirstOrDefaultAsync(c => c.Id == contactId, ct);

    public void Add(Contact contact) => db.Contacts.Add(contact);
}

internal sealed class ConversationRepository(InboxDbContext db) : IConversationRepository
{
    public Task<Conversation?> GetAsync(Guid conversationId, CancellationToken ct) =>
        db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId, ct);

    public Task<Conversation?> FindOpenAsync(Guid channelId, Guid contactId, CancellationToken ct) =>
        db.Conversations
            .Where(c => c.ChannelId == channelId && c.ContactId == contactId && c.Status != ConversationStatus.Closed)
            .OrderByDescending(c => c.LastActivityAtUtc)
            .FirstOrDefaultAsync(ct);

    public void Add(Conversation conversation) => db.Conversations.Add(conversation);
}

internal sealed class MessageRepository(InboxDbContext db) : IMessageRepository
{
    public Task<Message?> GetAsync(Guid messageId, CancellationToken ct) =>
        db.Messages.FirstOrDefaultAsync(m => m.Id == messageId, ct);

    public void Add(Message message) => db.Messages.Add(message);
}
