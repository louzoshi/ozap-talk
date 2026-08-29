using Microsoft.EntityFrameworkCore;

namespace SopaTalk.Channels.Infrastructure.Persistence;

/// <summary>
/// Owns every table under the <c>channels</c> Postgres schema. No other module's DbContext may
/// map a type in this schema, and this context never maps a type outside it.
/// </summary>
public sealed class ChannelsDbContext(DbContextOptions<ChannelsDbContext> options) : DbContext(options)
{
    public const string Schema = "channels";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChannelsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
