using SopaTalk.Accounts.Domain.Accounts;
using SopaTalk.Accounts.Domain.Invitations;
using SopaTalk.Accounts.Domain.Users;

namespace SopaTalk.Accounts.Application.Abstractions;

public interface IAccountRepository
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct);
    Task<Account?> GetAsync(Guid accountId, CancellationToken ct);
    void Add(Account account);
}

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct);

    /// <summary>Looked up across tenants — sign-in has no tenant scope yet.</summary>
    Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken ct);

    Task<User?> GetAsync(Guid userId, CancellationToken ct);

    /// <summary>Todos os usuários do tenant em escopo, ativos ou não.</summary>
    Task<IReadOnlyList<User>> ListByTenantAsync(CancellationToken ct);

    void Add(User user);
}

public interface IInvitationRepository
{
    /// <summary>Busca sem escopo de tenant — o aceite do convite é anônimo.</summary>
    Task<Invitation?> FindByTokenHashAsync(string tokenHash, CancellationToken ct);

    /// <summary>Há um convite pendente para este e-mail no tenant em escopo?</summary>
    Task<bool> HasPendingForEmailAsync(string normalizedEmail, CancellationToken ct);

    Task<IReadOnlyList<Invitation>> ListPendingAsync(CancellationToken ct);
    Task<Invitation?> GetAsync(Guid invitationId, CancellationToken ct);
    void Add(Invitation invitation);
}

/// <summary>Commits the module's unit of work (one transaction).</summary>
public interface IAccountsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
