using Microsoft.EntityFrameworkCore;
using SopaTalk.Accounts.Application.Abstractions;
using SopaTalk.Accounts.Domain.Accounts;
using SopaTalk.Accounts.Domain.Users;
using SopaTalk.SharedKernel.MultiTenancy;
using SopaTalk.SharedKernel.Persistence;

namespace SopaTalk.Accounts.Infrastructure.Persistence;

public sealed class AccountsDbContext(DbContextOptions<AccountsDbContext> options, ITenantContext tenantContext)
    : TenantDbContext(options, tenantContext), IAccountsUnitOfWork
{
    public const string Schema = "accounts";

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountsDbContext).Assembly);

        // User is tenant-owned; Account IS the tenant, so it is not filtered.
        modelBuilder.Entity<User>().HasQueryFilter(u => u.TenantId == CurrentTenantId);

        base.OnModelCreating(modelBuilder);
    }

    // IAccountsUnitOfWork.SaveChangesAsync(CancellationToken) is satisfied by the
    // inherited DbContext.SaveChangesAsync(CancellationToken).
}
