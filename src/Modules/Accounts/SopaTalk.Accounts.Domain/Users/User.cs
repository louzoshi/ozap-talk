using System.Text.RegularExpressions;
using SopaTalk.SharedKernel.Domain;
using SopaTalk.SharedKernel.MultiTenancy;

namespace SopaTalk.Accounts.Domain.Users;

public enum MembershipRole
{
    /// <summary>Full access, including billing. Exactly one per account.</summary>
    Owner = 0,

    /// <summary>Everything except billing.</summary>
    Admin = 1,

    /// <summary>Handles conversations; no configuration.</summary>
    Agent = 2,
}

/// <summary>A person who signs in. Always belongs to exactly one account (tenant).</summary>
public sealed partial class User : AggregateRoot, ITenantOwned
{
    private User(Guid id, Guid tenantId, string email, string displayName, string passwordHash, MembershipRole role)
        : base(id)
    {
        TenantId = tenantId;
        Email = email;
        DisplayName = displayName;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    // EF Core
    private User() { }

    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public MembershipRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? LastSignedInAtUtc { get; private set; }

    public static User Create(Guid tenantId, string email, string displayName, string passwordHash, MembershipRole role)
    {
        var normalizedEmail = Normalize(email);
        if (!EmailPattern().IsMatch(normalizedEmail))
            throw new ArgumentException("E-mail inválido.", nameof(email));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Informe o nome do usuário.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Hash de senha ausente.", nameof(passwordHash));

        return new User(Guid.CreateVersion7(), tenantId, normalizedEmail, displayName.Trim(), passwordHash, role);
    }

    public void MarkSignedIn() => LastSignedInAtUtc = DateTimeOffset.UtcNow;

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Hash de senha ausente.", nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
    }

    public void Deactivate() => IsActive = false;

    public static string Normalize(string email) => email.Trim().ToLowerInvariant();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
