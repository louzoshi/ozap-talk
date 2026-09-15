using Microsoft.EntityFrameworkCore;

namespace OzapTalk.AiAgent.Infrastructure.Persistence;

/// <summary>
/// Owns every table under the <c>ai_agent</c> Postgres schema. No other module's DbContext may
/// map a type in this schema, and this context never maps a type outside it.
/// </summary>
public sealed class AiAgentDbContext(DbContextOptions<AiAgentDbContext> options) : DbContext(options)
{
    public const string Schema = "ai_agent";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AiAgentDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
