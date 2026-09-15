using OzapTalk.SharedKernel.Domain;
using OzapTalk.SharedKernel.MultiTenancy;

namespace OzapTalk.Channels.Domain.WhatsApp;

/// <summary>
/// One row per WhatsApp message we have already handled. Meta retries webhooks, so
/// every inbound message is checked against this table before it is processed.
/// </summary>
public sealed class ProcessedInboundMessage : Entity, ITenantOwned
{
    private ProcessedInboundMessage(Guid id, Guid tenantId, string providerMessageId) : base(id)
    {
        TenantId = tenantId;
        ProviderMessageId = providerMessageId;
        ProcessedAtUtc = DateTimeOffset.UtcNow;
    }

    // EF Core
    private ProcessedInboundMessage() { }

    public Guid TenantId { get; private set; }
    public string ProviderMessageId { get; private set; } = string.Empty;
    public DateTimeOffset ProcessedAtUtc { get; private set; }

    public static ProcessedInboundMessage Of(Guid tenantId, string providerMessageId) =>
        new(Guid.CreateVersion7(), tenantId, providerMessageId);
}
