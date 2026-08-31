using SopaTalk.Accounts.Application.Abstractions;
using SopaTalk.Accounts.Domain.Accounts;
using SopaTalk.Accounts.Domain.Invitations;
using SopaTalk.Accounts.Domain.Users;

namespace SopaTalk.Accounts.UnitTests.Team;

internal sealed class FakeUserRepository : IUserRepository
{
    public List<User> Users { get; } = [];

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct) =>
        Task.FromResult(Users.Any(u => u.Email == normalizedEmail));

    public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken ct) =>
        Task.FromResult(Users.FirstOrDefault(u => u.Email == normalizedEmail));

    public Task<User?> GetAsync(Guid userId, CancellationToken ct) =>
        Task.FromResult(Users.FirstOrDefault(u => u.Id == userId));

    public Task<IReadOnlyList<User>> ListByTenantAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<User>>(Users);

    public void Add(User user) => Users.Add(user);
}

internal sealed class FakeAccountRepository(Account account) : IAccountRepository
{
    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct) => Task.FromResult(false);

    public Task<Account?> GetAsync(Guid accountId, CancellationToken ct) =>
        Task.FromResult<Account?>(accountId == account.Id ? account : null);

    public void Add(Account a) { }
}

internal sealed class FakeInvitationRepository : IInvitationRepository
{
    public List<Invitation> Invitations { get; } = [];

    public Task<Invitation?> FindByTokenHashAsync(string tokenHash, CancellationToken ct) =>
        Task.FromResult(Invitations.FirstOrDefault(i => i.TokenHash == tokenHash));

    public Task<bool> HasPendingForEmailAsync(string normalizedEmail, CancellationToken ct) =>
        Task.FromResult(Invitations.Any(i => i.Email == normalizedEmail && i.Status == InvitationStatus.Pending));

    public Task<IReadOnlyList<Invitation>> ListPendingAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Invitation>>(
            Invitations.Where(i => i.Status == InvitationStatus.Pending).ToList());

    public Task<Invitation?> GetAsync(Guid invitationId, CancellationToken ct) =>
        Task.FromResult(Invitations.FirstOrDefault(i => i.Id == invitationId));

    public void Add(Invitation invitation) => Invitations.Add(invitation);
}

internal sealed class FakeUnitOfWork : IAccountsUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        SaveCount++;
        return Task.FromResult(0);
    }
}

internal sealed class FakeInvitationTokens : IInvitationTokens
{
    private int _n;

    public InvitationToken Create()
    {
        var raw = $"raw-token-{++_n}";
        return new InvitationToken(raw, Hash(raw));
    }

    public string Hash(string rawToken) => $"hash({rawToken})";
}

internal sealed class RecordingEmailSender : IInvitationEmailSender
{
    public InvitationEmail? Sent { get; private set; }

    public Task SendAsync(InvitationEmail email, CancellationToken ct)
    {
        Sent = email;
        return Task.CompletedTask;
    }
}

internal sealed class FakeLinkBuilder : IInviteLinkBuilder
{
    public string BuildAcceptUrl(string rawToken) => $"https://app.test/convite/{rawToken}";
}
