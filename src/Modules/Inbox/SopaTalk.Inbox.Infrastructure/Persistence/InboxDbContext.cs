using Microsoft.EntityFrameworkCore;

namespace SopaTalk.Inbox.Infrastructure.Persistence;

/// <summary>
/// Owns every table under the <c>inbox</c> Postgres schema. No other module's DbContext may
/// map a type in this schema, and this context never maps a type outside it.
/// </summary>
public sealed class InboxDbContext(DbContextOptions<InboxDbContext> options) : DbContext(options)
{
    public const string Schema = "inbox";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InboxDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
