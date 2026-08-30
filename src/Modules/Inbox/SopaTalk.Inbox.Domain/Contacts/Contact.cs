using SopaTalk.SharedKernel.Domain;
using SopaTalk.SharedKernel.MultiTenancy;

namespace SopaTalk.Inbox.Domain.Contacts;

/// <summary>A person the tenant talks to, identified by their WhatsApp phone number.</summary>
public sealed class Contact : AggregateRoot, ITenantOwned
{
    private Contact(Guid id, Guid tenantId, string phone, string? name) : base(id)
    {
        TenantId = tenantId;
        Phone = phone;
        Name = name;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    // EF Core
    private Contact() { }

    public Guid TenantId { get; private set; }
    public string Phone { get; private set; } = string.Empty;
    public string? Name { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Contact Create(Guid tenantId, string phone, string? name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);
        return new Contact(Guid.CreateVersion7(), tenantId, phone.Trim(), Clean(name));
    }

    /// <summary>Fills the name from the WhatsApp profile when we don't have one yet.</summary>
    public void EnrichName(string? name)
    {
        var clean = Clean(name);
        if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(clean))
            Name = clean;
    }

    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? Phone : Name!;

    private static string? Clean(string? name) =>
        string.IsNullOrWhiteSpace(name) ? null : name.Trim();
}
