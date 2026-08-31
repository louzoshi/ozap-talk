using SopaTalk.SharedKernel.Results;

namespace SopaTalk.Accounts.Domain.Invitations;

public static class InvitationErrors
{
    public static readonly Error EmailRequired =
        Error.Validation("invitation.email_required", "Informe o e-mail de quem você quer convidar.");

    public static readonly Error EmailInvalid =
        Error.Validation("invitation.email_invalid", "E-mail inválido.");

    public static readonly Error RoleNotAssignable =
        Error.Validation("invitation.role_not_assignable", "Não é possível convidar alguém como proprietário.");

    public static readonly Error Forbidden =
        new("invitation.forbidden", "Seu papel não permite gerenciar esta pessoa.", ErrorType.Forbidden);

    public static readonly Error EmailAlreadyInUse =
        Error.Conflict("invitation.email_in_use", "Já existe um usuário com este e-mail.");

    public static readonly Error PendingInvitationExists =
        Error.Conflict("invitation.pending_exists", "Já existe um convite pendente para este e-mail.");

    public static readonly Error NotFound =
        Error.NotFound("invitation.not_found", "Convite não encontrado.");

    public static readonly Error NotPending =
        Error.Validation("invitation.not_pending", "Este convite já foi usado ou cancelado.");

    public static readonly Error Expired =
        Error.Validation("invitation.expired", "Este convite expirou. Peça um novo ao administrador.");

    public static readonly Error CannotChangeOwnRole =
        Error.Validation("invitation.cannot_change_own_role", "Você não pode alterar o seu próprio papel.");

    public static readonly Error CannotRemoveSelf =
        Error.Validation("invitation.cannot_remove_self", "Você não pode remover a si mesmo.");

    public static readonly Error OwnerRoleImmutable =
        Error.Validation("invitation.owner_role_immutable", "O papel do proprietário não pode ser alterado.");
}
