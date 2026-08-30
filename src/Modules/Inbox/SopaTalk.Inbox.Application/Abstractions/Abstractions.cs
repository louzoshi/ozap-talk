using SopaTalk.Inbox.Domain.Contacts;
using SopaTalk.Inbox.Domain.Conversations;
using SopaTalk.Inbox.Domain.Messages;

namespace SopaTalk.Inbox.Application.Abstractions;

public interface IContactRepository
{
    Task<Contact?> FindByPhoneAsync(string phone, CancellationToken ct);
    Task<Contact?> GetAsync(Guid contactId, CancellationToken ct);
    void Add(Contact contact);
}

public interface IConversationRepository
{
    Task<Conversation?> GetAsync(Guid conversationId, CancellationToken ct);
    Task<Conversation?> FindOpenAsync(Guid channelId, Guid contactId, CancellationToken ct);
    void Add(Conversation conversation);
}

public interface IMessageRepository
{
    Task<Message?> GetAsync(Guid messageId, CancellationToken ct);
    void Add(Message message);
}

public interface IInboxUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
