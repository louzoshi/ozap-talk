using FluentAssertions;
using OzapTalk.Accounts.Domain.Accounts;
using OzapTalk.Accounts.Domain.Users;

namespace OzapTalk.Accounts.UnitTests;

public class RegistrationTests
{
    [Fact]
    public void Register_creates_account_and_owner_and_raises_event()
    {
        var result = Account.Register("Acme", "acme", "owner@acme.com", "Owner", "hash");

        result.IsSuccess.Should().BeTrue();
        var (account, owner) = result.Value;

        account.Slug.Should().Be("acme");
        account.Plan.Should().Be(PlanTier.Trial);
        owner.TenantId.Should().Be(account.Id);
        owner.Role.Should().Be(MembershipRole.Owner);
        owner.Email.Should().Be("owner@acme.com");
        account.DomainEvents.Should().ContainSingle(e => e is AccountRegisteredEvent);
    }

    [Fact]
    public void Register_rejects_blank_company_name()
    {
        var result = Account.Register(" ", "acme", "owner@acme.com", "Owner", "hash");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.CompanyNameRequired);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@acme.com")]
    public void User_create_rejects_invalid_email(string email)
    {
        var act = () => User.Create(Guid.NewGuid(), email, "Name", "hash", MembershipRole.Operator);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void User_email_is_normalized_to_lowercase()
    {
        var user = User.Create(Guid.NewGuid(), "  Owner@ACME.com ", "Name", "hash", MembershipRole.Operator);

        user.Email.Should().Be("owner@acme.com");
    }
}
