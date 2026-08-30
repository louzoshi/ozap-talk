using SopaTalk.Channels.Domain.WhatsApp;

namespace SopaTalk.Channels.Application.Abstractions;

/// <summary>Talks to the WhatsApp Cloud API. Implemented in Infrastructure over HttpClient.</summary>
public interface IWhatsAppApi
{
    /// <summary>Sends a free-form text message. Returns the provider message id (<c>wamid...</c>).</summary>
    Task<string> SendTextAsync(string phoneNumberId, string accessToken, string toPhone, string text, CancellationToken ct);
}

public interface IChannelRepository
{
    /// <summary>Webhook lookup — no tenant in scope, so this bypasses the tenant filter.</summary>
    Task<WhatsAppChannel?> FindByPhoneNumberIdAsync(string phoneNumberId, CancellationToken ct);

    Task<WhatsAppChannel?> GetAsync(Guid channelId, CancellationToken ct);
    Task<IReadOnlyList<WhatsAppChannel>> ListAsync(CancellationToken ct);
    Task<bool> ExistsForPhoneNumberIdAsync(string phoneNumberId, CancellationToken ct);
    void Add(WhatsAppChannel channel);
}

public interface IProcessedMessageStore
{
    Task<bool> AlreadyProcessedAsync(Guid tenantId, string providerMessageId, CancellationToken ct);
    void MarkProcessed(ProcessedInboundMessage processed);
}

public interface IChannelsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
