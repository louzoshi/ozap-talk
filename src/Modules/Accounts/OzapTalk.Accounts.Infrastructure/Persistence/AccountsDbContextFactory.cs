using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OzapTalk.SharedKernel.MultiTenancy;

namespace OzapTalk.Accounts.Infrastructure.Persistence;

/// <summary>Used by `dotnet ef` at design time. Never runs at application runtime.</summary>
internal sealed class AccountsDbContextFactory : IDesignTimeDbContextFactory<AccountsDbContext>
{
    public AccountsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseNpgsql("Host=localhost;Database=ozaptalk;Username=ozaptalk;Password=ozaptalk",
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", AccountsDbContext.Schema))
            .Options;

        return new AccountsDbContext(options, new TenantContext());
    }
}
