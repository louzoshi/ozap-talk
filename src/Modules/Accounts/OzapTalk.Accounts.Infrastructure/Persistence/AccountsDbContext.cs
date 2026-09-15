using Microsoft.EntityFrameworkCore;
using OzapTalk.Accounts.Application.Abstractions;
using OzapTalk.Accounts.Domain.Accounts;
using OzapTalk.Accounts.Domain.Invitations;
using OzapTalk.Accounts.Domain.Users;
using OzapTalk.SharedKernel.MultiTenancy;
using OzapTalk.SharedKernel.Persistence;

namespace OzapTalk.Accounts.Infrastructure.Persistence;

public sealed class AccountsDbContext(DbContextOptions<AccountsDbContext> options, ITenantContext tenantContext)
    : TenantDbContext(options, tenantContext), IAccountsUnitOfWork
{
    public const string Schema = "accounts";

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Invitation> Invitations => Set<Invitation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountsDbContext).Assembly);

        // User is tenant-owned; Account IS the tenant, so it is not filtered.
        modelBuilder.Entity<User>().HasQueryFilter(u => u.TenantId == CurrentTenantId);
        modelBuilder.Entity<Invitation>().HasQueryFilter(i => i.TenantId == CurrentTenantId);

        base.OnModelCreating(modelBuilder);
    }

    // IAccountsUnitOfWork.SaveChangesAsync(CancellationToken) is satisfied by the
    // inherited DbContext.SaveChangesAsync(CancellationToken).
}
