using SopaTalk.Accounts.Domain.Users;
using SopaTalk.SharedKernel.Domain;
using SopaTalk.SharedKernel.MultiTenancy;
using SopaTalk.SharedKernel.Results;

namespace SopaTalk.Accounts.Domain.Invitations;

public enum InvitationStatus
{
    Pending = 0,
    Accepted = 1,
    Revoked = 2,
}

/// <summary>
/// Um convite para alguém entrar numa conta com um papel definido. O token cru vai no
/// link enviado por e-mail e nunca é persistido — guardamos só o hash. Aceitar o convite
/// cria o <see cref="User"/> correspondente.
/// </summary>
public sealed class Invitation : AggregateRoot, ITenantOwned
{
    private Invitation(
        Guid id, Guid tenantId, string email, MembershipRole role,
        Guid invitedByUserId, string tokenHash, DateTimeOffset createdAtUtc, DateTimeOffset expiresAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        Email = email;
        Role = role;
        InvitedByUserId = invitedByUserId;
        TokenHash = tokenHash;
        Status = InvitationStatus.Pending;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    // EF Core
    private Invitation() { }

    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public MembershipRole Role { get; private set; }
    public Guid InvitedByUserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public InvitationStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset? AcceptedAtUtc { get; private set; }

    public static Result<Invitation> Create(
        Guid tenantId, string email, MembershipRole role, Guid invitedByUserId, string tokenHash, TimeSpan ttl)
    {
        if (string.IsNullOrWhiteSpace(email))
            return InvitationErrors.EmailRequired;
        if (!User.IsValidEmail(email))
            return InvitationErrors.EmailInvalid;
        if (role == MembershipRole.Owner)
            return InvitationErrors.RoleNotAssignable;

        var now = DateTimeOffset.UtcNow;
        return new Invitation(
            Guid.CreateVersion7(), tenantId, User.Normalize(email), role, invitedByUserId, tokenHash, now, now.Add(ttl));
    }

    public bool IsUsable(DateTimeOffset now) => Status == InvitationStatus.Pending && now < ExpiresAtUtc;

    public Result Accept(DateTimeOffset now)
    {
        if (Status != InvitationStatus.Pending)
            return InvitationErrors.NotPending;
        if (now >= ExpiresAtUtc)
            return InvitationErrors.Expired;

        Status = InvitationStatus.Accepted;
        AcceptedAtUtc = now;
        return Result.Success();
    }

    public Result Revoke()
    {
        if (Status != InvitationStatus.Pending)
            return InvitationErrors.NotPending;

        Status = InvitationStatus.Revoked;
        return Result.Success();
    }
}
