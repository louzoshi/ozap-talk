namespace OzapTalk.Accounts.Domain.Users;

/// <summary>
/// Alçada entre papéis: quem pode convidar, promover, rebaixar ou remover quem.
/// O proprietário mexe em todos, menos em outro proprietário (não há dois, e a
/// transferência de propriedade é um fluxo à parte). O admin só mexe em operador e
/// membro. Operador e membro não gerenciam ninguém.
/// </summary>
public static class MembershipRules
{
    public static bool CanManage(MembershipRole actor, MembershipRole target) => actor switch
    {
        MembershipRole.Owner => target != MembershipRole.Owner,
        MembershipRole.Admin => target is MembershipRole.Operator or MembershipRole.Member,
        _ => false,
    };

    /// <summary>Papéis que <paramref name="actor"/> pode atribuir a outra pessoa.</summary>
    public static IReadOnlyList<MembershipRole> AssignableBy(MembershipRole actor) => actor switch
    {
        MembershipRole.Owner => [MembershipRole.Admin, MembershipRole.Operator, MembershipRole.Member],
        MembershipRole.Admin => [MembershipRole.Operator, MembershipRole.Member],
        _ => [],
    };
}
