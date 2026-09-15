using System.Text.RegularExpressions;
using OzapTalk.SharedKernel.Domain;
using OzapTalk.SharedKernel.MultiTenancy;

namespace OzapTalk.Accounts.Domain.Users;

public enum MembershipRole
{
    /// <summary>Acesso total, incluindo o painel financeiro. Exatamente um por conta.</summary>
    Owner = 0,

    /// <summary>Tudo na ferramenta, menos o painel financeiro.</summary>
    Admin = 1,

    /// <summary>Envia mensagens para contatos e ajusta configurações básicas de atendimento.</summary>
    Operator = 2,

    /// <summary>Somente leitura: não envia mensagens nem altera configurações.</summary>
    Member = 3,
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

    /// <summary>
    /// Troca o papel do usuário. O papel <see cref="MembershipRole.Owner"/> é definido só no
    /// registro da empresa (ou numa transferência de propriedade) e não pode ser atribuído aqui.
    /// </summary>
    public void ChangeRole(MembershipRole newRole)
    {
        if (newRole == MembershipRole.Owner)
            throw new ArgumentException("O papel de proprietário não pode ser atribuído desta forma.", nameof(newRole));
        Role = newRole;
    }

    public void Reactivate() => IsActive = true;

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Hash de senha ausente.", nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
    }

    public void Deactivate() => IsActive = false;

    public static string Normalize(string email) => email.Trim().ToLowerInvariant();

    public static bool IsValidEmail(string email) => EmailPattern().IsMatch(Normalize(email));

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
