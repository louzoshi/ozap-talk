using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SopaTalk.SharedKernel.MultiTenancy;

namespace SopaTalk.Inbox.Infrastructure.Persistence;

internal sealed class InboxDbContextFactory : IDesignTimeDbContextFactory<InboxDbContext>
{
    public InboxDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<InboxDbContext>()
            .UseNpgsql("Host=localhost;Database=sopatalk;Username=sopatalk;Password=sopatalk",
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", InboxDbContext.Schema))
            .Options;

        return new InboxDbContext(options, new TenantContext());
    }
}
