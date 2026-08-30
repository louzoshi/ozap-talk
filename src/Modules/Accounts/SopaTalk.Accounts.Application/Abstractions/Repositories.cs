using SopaTalk.Accounts.Domain.Accounts;
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
    void Add(User user);
}

/// <summary>Commits the module's unit of work (one transaction).</summary>
public interface IAccountsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
