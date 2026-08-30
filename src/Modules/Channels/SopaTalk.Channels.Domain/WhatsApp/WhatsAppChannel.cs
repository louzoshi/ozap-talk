using SopaTalk.SharedKernel.Domain;
using SopaTalk.SharedKernel.MultiTenancy;

namespace SopaTalk.Channels.Domain.WhatsApp;

/// <summary>
/// One WhatsApp Cloud API phone number connected to a tenant. The webhook verify token
/// and app secret are app-level (Channels configuration); only the per-number id and
/// access token live here.
/// </summary>
public sealed class WhatsAppChannel : AggregateRoot, ITenantOwned
{
    private WhatsAppChannel(
        Guid id, Guid tenantId, string phoneNumberId, string displayPhoneNumber,
        string wabaId, string accessToken) : base(id)
    {
        TenantId = tenantId;
        PhoneNumberId = phoneNumberId;
        DisplayPhoneNumber = displayPhoneNumber;
        WabaId = wabaId;
        AccessToken = accessToken;
        IsActive = true;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    // EF Core
    private WhatsAppChannel() { }

    public Guid TenantId { get; private set; }

    /// <summary>Meta's <c>phone_number_id</c> — the routing key for inbound webhooks.</summary>
    public string PhoneNumberId { get; private set; } = string.Empty;
    public string DisplayPhoneNumber { get; private set; } = string.Empty;
    public string WabaId { get; private set; } = string.Empty;

    // TODO(R0-security): encrypt at rest / move to a secret store.
    public string AccessToken { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static WhatsAppChannel Connect(
        Guid tenantId, string phoneNumberId, string displayPhoneNumber, string wabaId, string accessToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumberId);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

        return new WhatsAppChannel(
            Guid.CreateVersion7(), tenantId, phoneNumberId.Trim(),
            displayPhoneNumber?.Trim() ?? string.Empty, wabaId?.Trim() ?? string.Empty, accessToken.Trim());
    }

    public void UpdateAccessToken(string accessToken)
    {
        if (!string.IsNullOrWhiteSpace(accessToken))
            AccessToken = accessToken.Trim();
    }

    public void Disable() => IsActive = false;
}
