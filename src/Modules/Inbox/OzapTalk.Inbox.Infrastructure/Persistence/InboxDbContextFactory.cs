using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OzapTalk.SharedKernel.MultiTenancy;

namespace OzapTalk.Inbox.Infrastructure.Persistence;

internal sealed class InboxDbContextFactory : IDesignTimeDbContextFactory<InboxDbContext>
{
    public InboxDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<InboxDbContext>()
            .UseNpgsql("Host=localhost;Database=ozaptalk;Username=ozaptalk;Password=ozaptalk",
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", InboxDbContext.Schema))
            .Options;

        return new InboxDbContext(options, new TenantContext());
    }
}
