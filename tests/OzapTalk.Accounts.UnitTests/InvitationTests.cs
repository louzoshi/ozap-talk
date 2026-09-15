using FluentAssertions;
using OzapTalk.Accounts.Domain.Invitations;
using OzapTalk.Accounts.Domain.Users;

namespace OzapTalk.Accounts.UnitTests;

public class InvitationTests
{
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(72);

    [Fact]
    public void Create_normalizes_email_and_starts_pending()
    {
        var result = Invitation.Create(Guid.NewGuid(), "  New@Acme.com ", MembershipRole.Operator, Guid.NewGuid(), "hash", Ttl);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("new@acme.com");
        result.Value.Status.Should().Be(InvitationStatus.Pending);
        result.Value.ExpiresAtUtc.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Create_rejects_owner_role()
    {
        var result = Invitation.Create(Guid.NewGuid(), "new@acme.com", MembershipRole.Owner, Guid.NewGuid(), "hash", Ttl);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvitationErrors.RoleNotAssignable);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    public void Create_rejects_invalid_email(string email)
    {
        var result = Invitation.Create(Guid.NewGuid(), email, MembershipRole.Member, Guid.NewGuid(), "hash", Ttl);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Accept_moves_pending_to_accepted()
    {
        var invitation = NewPending();

        var result = invitation.Accept(DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        invitation.Status.Should().Be(InvitationStatus.Accepted);
        invitation.AcceptedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Accept_fails_when_expired()
    {
        var invitation = Invitation.Create(
            Guid.NewGuid(), "new@acme.com", MembershipRole.Member, Guid.NewGuid(), "hash", TimeSpan.FromSeconds(-1)).Value;

        var result = invitation.Accept(DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvitationErrors.Expired);
        invitation.Status.Should().Be(InvitationStatus.Pending);
    }

    [Fact]
    public void Accept_fails_after_revoke()
    {
        var invitation = NewPending();
        invitation.Revoke();

        invitation.Accept(DateTimeOffset.UtcNow).Error.Should().Be(InvitationErrors.NotPending);
    }

    [Fact]
    public void Revoke_twice_fails()
    {
        var invitation = NewPending();
        invitation.Revoke();

        invitation.Revoke().Error.Should().Be(InvitationErrors.NotPending);
    }

    private static Invitation NewPending() =>
        Invitation.Create(Guid.NewGuid(), "new@acme.com", MembershipRole.Operator, Guid.NewGuid(), "hash", Ttl).Value;
}
