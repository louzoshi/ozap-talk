using Microsoft.EntityFrameworkCore;
using SopaTalk.Accounts.Application.Abstractions;
using SopaTalk.Accounts.Domain.Accounts;

namespace SopaTalk.Accounts.Infrastructure.Persistence;

internal sealed class AccountRepository(AccountsDbContext db) : IAccountRepository
{
    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct) =>
        db.Accounts.AnyAsync(a => a.Slug == slug, ct);

    public Task<Account?> GetAsync(Guid accountId, CancellationToken ct) =>
        db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, ct);

    public void Add(Account account) => db.Accounts.Add(account);
}
