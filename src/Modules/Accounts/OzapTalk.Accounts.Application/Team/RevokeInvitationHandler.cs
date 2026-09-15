using OzapTalk.Accounts.Application.Abstractions;
using OzapTalk.Accounts.Domain.Invitations;
using OzapTalk.Accounts.Domain.Users;
using OzapTalk.SharedKernel.Results;

namespace OzapTalk.Accounts.Application.Team;

public sealed record RevokeInvitationCommand(Guid ActorUserId, Guid InvitationId);

public sealed class RevokeInvitationHandler(
    IUserRepository users,
    IInvitationRepository invitations,
    IAccountsUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(RevokeInvitationCommand command, CancellationToken ct)
    {
        var actor = await users.GetAsync(command.ActorUserId, ct);
        if (actor is null)
            return TeamErrors.ActorNotFound;

        var invitation = await invitations.GetAsync(command.InvitationId, ct);
        if (invitation is null)
            return InvitationErrors.NotFound;

        if (!MembershipRules.CanManage(actor.Role, invitation.Role))
            return InvitationErrors.Forbidden;

        var result = invitation.Revoke();
        if (result.IsFailure)
            return result.Error;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
