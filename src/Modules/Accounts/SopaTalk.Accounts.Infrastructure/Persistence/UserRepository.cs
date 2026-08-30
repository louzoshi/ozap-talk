using Microsoft.EntityFrameworkCore;
using SopaTalk.Accounts.Application.Abstractions;
using SopaTalk.Accounts.Domain.Users;

namespace SopaTalk.Accounts.Infrastructure.Persistence;

internal sealed class UserRepository(AccountsDbContext db) : IUserRepository
{
    // Registration and sign-in have no tenant scope, so these two lookups deliberately
    // bypass the tenant query filter. Every other read stays filtered.
    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct) =>
        db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == normalizedEmail, ct);

    public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken ct) =>
        db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

    public Task<User?> GetAsync(Guid userId, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

    public void Add(User user) => db.Users.Add(user);
}
