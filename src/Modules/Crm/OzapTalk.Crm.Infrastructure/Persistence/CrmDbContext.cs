using Microsoft.EntityFrameworkCore;

namespace OzapTalk.Crm.Infrastructure.Persistence;

/// <summary>
/// Owns every table under the <c>crm</c> Postgres schema. No other module's DbContext may
/// map a type in this schema, and this context never maps a type outside it.
/// </summary>
public sealed class CrmDbContext(DbContextOptions<CrmDbContext> options) : DbContext(options)
{
    public const string Schema = "crm";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
