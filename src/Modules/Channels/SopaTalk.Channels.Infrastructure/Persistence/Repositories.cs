using Microsoft.EntityFrameworkCore;
using SopaTalk.Channels.Application.Abstractions;
using SopaTalk.Channels.Domain.WhatsApp;

namespace SopaTalk.Channels.Infrastructure.Persistence;

internal sealed class ChannelRepository(ChannelsDbContext db) : IChannelRepository
{
    // No tenant in scope during webhook handling — bypass the filter to route by number.
    public Task<WhatsAppChannel?> FindByPhoneNumberIdAsync(string phoneNumberId, CancellationToken ct) =>
        db.Channels.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.PhoneNumberId == phoneNumberId, ct);

    public Task<WhatsAppChannel?> GetAsync(Guid channelId, CancellationToken ct) =>
        db.Channels.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == channelId, ct);

    public async Task<IReadOnlyList<WhatsAppChannel>> ListAsync(CancellationToken ct) =>
        await db.Channels.OrderBy(c => c.CreatedAtUtc).ToListAsync(ct);

    public Task<bool> ExistsForPhoneNumberIdAsync(string phoneNumberId, CancellationToken ct) =>
        db.Channels.IgnoreQueryFilters().AnyAsync(c => c.PhoneNumberId == phoneNumberId, ct);

    public void Add(WhatsAppChannel channel) => db.Channels.Add(channel);
}

internal sealed class ProcessedMessageStore(ChannelsDbContext db) : IProcessedMessageStore
{
    public Task<bool> AlreadyProcessedAsync(Guid tenantId, string providerMessageId, CancellationToken ct) =>
        db.ProcessedMessages.IgnoreQueryFilters()
            .AnyAsync(p => p.TenantId == tenantId && p.ProviderMessageId == providerMessageId, ct);

    public void MarkProcessed(ProcessedInboundMessage processed) => db.ProcessedMessages.Add(processed);
}
