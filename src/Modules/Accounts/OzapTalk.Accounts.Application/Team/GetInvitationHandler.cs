using OzapTalk.Accounts.Application.Abstractions;
using OzapTalk.Accounts.Domain.Accounts;
using OzapTalk.Accounts.Domain.Invitations;
using OzapTalk.SharedKernel.Results;

namespace OzapTalk.Accounts.Application.Team;

/// <summary>Prévia pública da tela de aceite: qual empresa, qual e-mail, qual papel.</summary>
public sealed class GetInvitationHandler(
    IInvitationRepository invitations,
    IAccountRepository accounts,
    IInvitationTokens tokens)
{
    public async Task<Result<InvitationPreview>> HandleAsync(string rawToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return InvitationErrors.NotFound;

        var invitation = await invitations.FindByTokenHashAsync(tokens.Hash(rawToken), ct);
        if (invitation is null)
            return InvitationErrors.NotFound;
        if (invitation.Status != InvitationStatus.Pending)
            return InvitationErrors.NotPending;
        if (!invitation.IsUsable(DateTimeOffset.UtcNow))
            return InvitationErrors.Expired;

        var account = await accounts.GetAsync(invitation.TenantId, ct);
        if (account is null)
            return AccountErrors.NotFound;

        return new InvitationPreview(account.Name, invitation.Email, invitation.Role);
    }
}
