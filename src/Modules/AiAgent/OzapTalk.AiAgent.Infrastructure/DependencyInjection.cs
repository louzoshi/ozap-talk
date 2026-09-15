using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OzapTalk.AiAgent.Infrastructure.Persistence;

namespace OzapTalk.AiAgent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAiAgentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("ConnectionStrings:Database is not configured.");

        services.AddDbContext<AiAgentDbContext>(options => options
            .UseNpgsql(connectionString, npgsql => npgsql
                .MigrationsHistoryTable("__ef_migrations_history", AiAgentDbContext.Schema)));

        // TODO: register repositories and integration-event handlers for this module here.
        return services;
    }
}
