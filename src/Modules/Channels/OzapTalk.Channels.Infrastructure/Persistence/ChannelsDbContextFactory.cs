using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OzapTalk.SharedKernel.MultiTenancy;

namespace OzapTalk.Channels.Infrastructure.Persistence;

internal sealed class ChannelsDbContextFactory : IDesignTimeDbContextFactory<ChannelsDbContext>
{
    public ChannelsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ChannelsDbContext>()
            .UseNpgsql("Host=localhost;Database=ozaptalk;Username=ozaptalk;Password=ozaptalk",
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", ChannelsDbContext.Schema))
            .Options;

        return new ChannelsDbContext(options, new TenantContext());
    }
}
