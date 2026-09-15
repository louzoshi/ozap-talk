using FluentValidation;
using OzapTalk.Accounts.Application.Abstractions;
using OzapTalk.Accounts.Domain.Invitations;
using OzapTalk.Accounts.Domain.Users;
using OzapTalk.SharedKernel.Results;

namespace OzapTalk.Accounts.Application.Team;

public sealed record ChangeMemberRoleCommand(Guid ActorUserId, Guid TargetUserId, MembershipRole NewRole);

public sealed class ChangeMemberRoleCommandValidator : AbstractValidator<ChangeMemberRoleCommand>
{
    public ChangeMemberRoleCommandValidator()
    {
        RuleFor(x => x.NewRole).IsInEnum();
    }
}

public sealed class ChangeMemberRoleHandler(
    IUserRepository users,
    IAccountsUnitOfWork unitOfWork,
    IValidator<ChangeMemberRoleCommand> validator)
{
    public async Task<Result> HandleAsync(ChangeMemberRoleCommand command, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return Error.Validation("team.invalid", validation.Errors[0].ErrorMessage);

        if (command.ActorUserId == command.TargetUserId)
            return InvitationErrors.CannotChangeOwnRole;
        if (command.NewRole == MembershipRole.Owner)
            return InvitationErrors.RoleNotAssignable;

        var actor = await users.GetAsync(command.ActorUserId, ct);
        if (actor is null)
            return TeamErrors.ActorNotFound;

        var target = await users.GetAsync(command.TargetUserId, ct);
        if (target is null || target.TenantId != actor.TenantId)
            return TeamErrors.MemberNotFound;

        if (target.Role == MembershipRole.Owner)
            return InvitationErrors.OwnerRoleImmutable;

        // O ator precisa de alçada sobre o papel atual e sobre o novo papel.
        if (!MembershipRules.CanManage(actor.Role, target.Role) ||
            !MembershipRules.CanManage(actor.Role, command.NewRole))
            return InvitationErrors.Forbidden;

        if (target.Role == command.NewRole)
            return Result.Success();

        target.ChangeRole(command.NewRole);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
