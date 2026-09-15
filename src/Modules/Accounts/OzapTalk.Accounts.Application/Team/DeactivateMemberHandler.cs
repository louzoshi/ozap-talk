using OzapTalk.Accounts.Application.Abstractions;
using OzapTalk.Accounts.Domain.Invitations;
using OzapTalk.Accounts.Domain.Users;
using OzapTalk.SharedKernel.Results;

namespace OzapTalk.Accounts.Application.Team;

public sealed record DeactivateMemberCommand(Guid ActorUserId, Guid TargetUserId);

public sealed class DeactivateMemberHandler(IUserRepository users, IAccountsUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(DeactivateMemberCommand command, CancellationToken ct)
    {
        if (command.ActorUserId == command.TargetUserId)
            return InvitationErrors.CannotRemoveSelf;

        var actor = await users.GetAsync(command.ActorUserId, ct);
        if (actor is null)
            return TeamErrors.ActorNotFound;

        var target = await users.GetAsync(command.TargetUserId, ct);
        if (target is null || target.TenantId != actor.TenantId)
            return TeamErrors.MemberNotFound;

        if (target.Role == MembershipRole.Owner)
            return InvitationErrors.OwnerRoleImmutable;
        if (!MembershipRules.CanManage(actor.Role, target.Role))
            return InvitationErrors.Forbidden;

        if (target.IsActive)
        {
            target.Deactivate();
            await unitOfWork.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
