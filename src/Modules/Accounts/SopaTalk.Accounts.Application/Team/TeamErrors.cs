using SopaTalk.SharedKernel.Results;

namespace SopaTalk.Accounts.Application.Team;

public static class TeamErrors
{
    public static readonly Error ActorNotFound =
        new("team.actor_not_found", "Usuário não encontrado.", ErrorType.Unauthorized);

    public static readonly Error MemberNotFound =
        Error.NotFound("team.member_not_found", "Membro não encontrado.");
}
