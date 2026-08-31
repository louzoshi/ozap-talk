using FluentAssertions;
using SopaTalk.Accounts.Domain.Users;

namespace SopaTalk.Accounts.UnitTests;

public class MembershipRulesTests
{
    [Theory]
    [InlineData(MembershipRole.Owner, MembershipRole.Admin, true)]
    [InlineData(MembershipRole.Owner, MembershipRole.Operator, true)]
    [InlineData(MembershipRole.Owner, MembershipRole.Member, true)]
    [InlineData(MembershipRole.Owner, MembershipRole.Owner, false)]
    [InlineData(MembershipRole.Admin, MembershipRole.Operator, true)]
    [InlineData(MembershipRole.Admin, MembershipRole.Member, true)]
    [InlineData(MembershipRole.Admin, MembershipRole.Admin, false)]
    [InlineData(MembershipRole.Admin, MembershipRole.Owner, false)]
    [InlineData(MembershipRole.Operator, MembershipRole.Member, false)]
    [InlineData(MembershipRole.Member, MembershipRole.Member, false)]
    public void CanManage(MembershipRole actor, MembershipRole target, bool expected)
    {
        MembershipRules.CanManage(actor, target).Should().Be(expected);
    }

    [Fact]
    public void AssignableBy_owner_excludes_owner()
    {
        MembershipRules.AssignableBy(MembershipRole.Owner)
            .Should().BeEquivalentTo([MembershipRole.Admin, MembershipRole.Operator, MembershipRole.Member]);
    }

    [Fact]
    public void AssignableBy_admin_is_operator_and_member()
    {
        MembershipRules.AssignableBy(MembershipRole.Admin)
            .Should().BeEquivalentTo([MembershipRole.Operator, MembershipRole.Member]);
    }

    [Fact]
    public void AssignableBy_operator_is_empty()
    {
        MembershipRules.AssignableBy(MembershipRole.Operator).Should().BeEmpty();
    }
}
