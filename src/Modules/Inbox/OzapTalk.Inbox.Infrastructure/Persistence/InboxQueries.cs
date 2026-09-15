using Microsoft.EntityFrameworkCore;
using OzapTalk.Inbox.Application.Queries;
using OzapTalk.Inbox.Domain.Conversations;

namespace OzapTalk.Inbox.Infrastructure.Persistence;

internal sealed class InboxQueries(InboxDbContext db) : IInboxQueries
{
    public async Task<IReadOnlyList<ConversationListItem>> ListConversationsAsync(string? status, CancellationToken ct)
    {
        var query =
            from c in db.Conversations
            join ct2 in db.Contacts on c.ContactId equals ct2.Id
            select new { c, ct2 };

        if (Enum.TryParse<ConversationStatus>(status, ignoreCase: true, out var parsed))
            query = query.Where(x => x.c.Status == parsed);
        else
            query = query.Where(x => x.c.Status != ConversationStatus.Closed);

        return await query
            .OrderByDescending(x => x.c.LastActivityAtUtc)
            .Take(200)
            .Select(x => new ConversationListItem(
                x.c.Id,
                x.c.ContactId,
                x.ct2.Name ?? x.ct2.Phone,
                x.ct2.Phone,
                x.c.LastMessagePreview,
                x.c.LastActivityAtUtc,
                x.c.UnreadCount,
                x.c.Status.ToString(),
                x.c.AssignedAgentId))
            .ToListAsync(ct);
    }

    public async Task<ConversationThread?> GetThreadAsync(Guid conversationId, CancellationToken ct)
    {
        var header = await (
            from c in db.Conversations
            join ct2 in db.Contacts on c.ContactId equals ct2.Id
            where c.Id == conversationId
            select new { c, ct2 }).FirstOrDefaultAsync(ct);

        if (header is null)
            return null;

        var messages = await db.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => new MessageView(
                m.Id, m.Direction.ToString(), m.Status.ToString(), m.Body, m.AuthorAgentId, m.CreatedAtUtc))
            .ToListAsync(ct);

        return new ConversationThread(
            header.c.Id,
            header.ct2.Name ?? header.ct2.Phone,
            header.ct2.Phone,
            header.c.Status.ToString(),
            header.c.AssignedAgentId,
            messages);
    }
}
