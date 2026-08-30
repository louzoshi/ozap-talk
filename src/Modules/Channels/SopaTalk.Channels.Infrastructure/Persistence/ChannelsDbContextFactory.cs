using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SopaTalk.SharedKernel.MultiTenancy;

namespace SopaTalk.Channels.Infrastructure.Persistence;

internal sealed class ChannelsDbContextFactory : IDesignTimeDbContextFactory<ChannelsDbContext>
{
    public ChannelsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ChannelsDbContext>()
            .UseNpgsql("Host=localhost;Database=sopatalk;Username=sopatalk;Password=sopatalk",
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", ChannelsDbContext.Schema))
            .Options;

        return new ChannelsDbContext(options, new TenantContext());
    }
}
