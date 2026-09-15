using OzapTalk.Accounts.Domain.Users;

namespace OzapTalk.Accounts.Application.Team;

public sealed record MemberSummary(
    Guid Id,
    string Email,
    string DisplayName,
    MembershipRole Role,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastSignedInAtUtc);

public sealed record InvitationSummary(
    Guid Id,
    string Email,
    MembershipRole Role,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc);

/// <summary>Retornado só na criação — carrega o link de aceite (stub de e-mail em dev).</summary>
public sealed record InvitationCreated(InvitationSummary Invitation, string AcceptUrl);

public sealed record InvitationPreview(string CompanyName, string Email, MembershipRole Role);
