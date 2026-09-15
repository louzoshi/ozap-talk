using Microsoft.EntityFrameworkCore;

namespace OzapTalk.Chatbot.Infrastructure.Persistence;

/// <summary>
/// Owns every table under the <c>chatbot</c> Postgres schema. No other module's DbContext may
/// map a type in this schema, and this context never maps a type outside it.
/// </summary>
public sealed class ChatbotDbContext(DbContextOptions<ChatbotDbContext> options) : DbContext(options)
{
    public const string Schema = "chatbot";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatbotDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
