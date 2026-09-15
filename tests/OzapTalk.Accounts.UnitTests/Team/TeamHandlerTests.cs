using FluentAssertions;
using Microsoft.Extensions.Options;
using OzapTalk.Accounts.Application.Team;
using OzapTalk.Accounts.Domain.Accounts;
using OzapTalk.Accounts.Domain.Invitations;
using OzapTalk.Accounts.Domain.Users;
using OzapTalk.Accounts.Infrastructure.Security;

namespace OzapTalk.Accounts.UnitTests.Team;

public class TeamHandlerTests
{
    private readonly Account _account;
    private readonly User _owner;
    private readonly FakeUserRepository _users = new();
    private readonly FakeInvitationRepository _invitations = new();
    private readonly FakeUnitOfWork _uow = new();
    private readonly FakeInvitationTokens _tokens = new();
    private readonly RecordingEmailSender _email = new();

    public TeamHandlerTests()
    {
        (_account, _owner) = Account.Register("Acme", "acme", "owner@acme.com", "Owner", "hash").Value;
        _users.Add(_owner);
    }

    private User AddUser(MembershipRole role, string email)
    {
        var user = User.Create(_account.Id, email, email, "hash", role);
        _users.Add(user);
        return user;
    }

    private InviteMemberHandler NewInviteHandler() => new(
        _users, new FakeAccountRepository(_account), _invitations, _tokens, _email,
        new FakeLinkBuilder(), _uow, new InviteMemberCommandValidator());

    // --- InviteMemberHandler ---

    [Fact]
    public async Task Owner_can_invite_an_admin_and_link_is_returned()
    {
        var result = await NewInviteHandler().HandleAsync(
            new InviteMemberCommand(_owner.Id, "new@acme.com", MembershipRole.Admin), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.AcceptUrl.Should().Contain("/convite/");
        _email.Sent!.ToEmail.Should().Be("new@acme.com");
        _invitations.Invitations.Should().ContainSingle();
    }

    [Fact]
    public async Task Admin_cannot_invite_an_admin()
    {
        var admin = AddUser(MembershipRole.Admin, "admin@acme.com");

        var result = await NewInviteHandler().HandleAsync(
            new InviteMemberCommand(admin.Id, "new@acme.com", MembershipRole.Admin), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvitationErrors.Forbidden);
    }

    [Fact]
    public async Task Cannot_invite_as_owner()
    {
        var result = await NewInviteHandler().HandleAsync(
            new InviteMemberCommand(_owner.Id, "new@acme.com", MembershipRole.Owner), default);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Cannot_invite_an_existing_user()
    {
        var result = await NewInviteHandler().HandleAsync(
            new InviteMemberCommand(_owner.Id, "owner@acme.com", MembershipRole.Operator), default);

        result.Error.Should().Be(InvitationErrors.EmailAlreadyInUse);
    }

    [Fact]
    public async Task Cannot_invite_twice()
    {
        await NewInviteHandler().HandleAsync(
            new InviteMemberCommand(_owner.Id, "new@acme.com", MembershipRole.Operator), default);

        var second = await NewInviteHandler().HandleAsync(
            new InviteMemberCommand(_owner.Id, "new@acme.com", MembershipRole.Operator), default);

        second.Error.Should().Be(InvitationErrors.PendingInvitationExists);
    }

    // --- AcceptInvitationHandler ---

    [Fact]
    public async Task Accept_creates_user_with_invited_role_and_marks_accepted()
    {
        var invite = await NewInviteHandler().HandleAsync(
            new InviteMemberCommand(_owner.Id, "op@acme.com", MembershipRole.Operator), default);
        var rawToken = invite.Value.AcceptUrl.Split('/').Last();

        var handler = new AcceptInvitationHandler(
            _invitations, _users, new FakeAccountRepository(_account), _tokens,
            new Pbkdf2PasswordHasher(), TokenIssuer(), _uow, new AcceptInvitationCommandValidator());

        var result = await handler.HandleAsync(
            new AcceptInvitationCommand(rawToken, "Operador", "supersecret"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.User.Role.Should().Be(nameof(MembershipRole.Operator));
        _users.Users.Should().Contain(u => u.Email == "op@acme.com" && u.Role == MembershipRole.Operator);
        _invitations.Invitations.Single().Status.Should().Be(InvitationStatus.Accepted);
    }

    [Fact]
    public async Task Accept_fails_for_unknown_token()
    {
        var handler = new AcceptInvitationHandler(
            _invitations, _users, new FakeAccountRepository(_account), _tokens,
            new Pbkdf2PasswordHasher(), TokenIssuer(), _uow, new AcceptInvitationCommandValidator());

        var result = await handler.HandleAsync(
            new AcceptInvitationCommand("nope", "Name", "supersecret"), default);

        result.Error.Should().Be(InvitationErrors.NotFound);
    }

    // --- ChangeMemberRoleHandler ---

    private ChangeMemberRoleHandler NewChangeHandler() =>
        new(_users, _uow, new ChangeMemberRoleCommandValidator());

    [Fact]
    public async Task Owner_changes_operator_to_member()
    {
        var op = AddUser(MembershipRole.Operator, "op@acme.com");

        var result = await NewChangeHandler().HandleAsync(
            new ChangeMemberRoleCommand(_owner.Id, op.Id, MembershipRole.Member), default);

        result.IsSuccess.Should().BeTrue();
        op.Role.Should().Be(MembershipRole.Member);
    }

    [Fact]
    public async Task Cannot_change_own_role()
    {
        var result = await NewChangeHandler().HandleAsync(
            new ChangeMemberRoleCommand(_owner.Id, _owner.Id, MembershipRole.Admin), default);

        result.Error.Should().Be(InvitationErrors.CannotChangeOwnRole);
    }

    [Fact]
    public async Task Admin_cannot_touch_another_admin()
    {
        var admin = AddUser(MembershipRole.Admin, "admin@acme.com");
        var other = AddUser(MembershipRole.Admin, "other@acme.com");

        var result = await NewChangeHandler().HandleAsync(
            new ChangeMemberRoleCommand(admin.Id, other.Id, MembershipRole.Operator), default);

        result.Error.Should().Be(InvitationErrors.Forbidden);
    }

    [Fact]
    public async Task Cannot_change_the_owner_role()
    {
        var admin = AddUser(MembershipRole.Admin, "admin@acme.com");

        var result = await NewChangeHandler().HandleAsync(
            new ChangeMemberRoleCommand(admin.Id, _owner.Id, MembershipRole.Admin), default);

        result.Error.Should().Be(InvitationErrors.OwnerRoleImmutable);
    }

    private static JwtAccessTokenIssuer TokenIssuer() => new(Options.Create(new JwtOptions
    {
        Issuer = "ozap-talk",
        Audience = "ozap-talk-api",
        SigningKey = "unit-test-signing-key-that-is-long-enough-32+",
        AccessTokenMinutes = 30,
    }));
}
