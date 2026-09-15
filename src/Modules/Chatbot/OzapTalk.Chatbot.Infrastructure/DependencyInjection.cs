using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OzapTalk.Chatbot.Infrastructure.Persistence;

namespace OzapTalk.Chatbot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddChatbotInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("ConnectionStrings:Database is not configured.");

        services.AddDbContext<ChatbotDbContext>(options => options
            .UseNpgsql(connectionString, npgsql => npgsql
                .MigrationsHistoryTable("__ef_migrations_history", ChatbotDbContext.Schema)));

        // TODO: register repositories and integration-event handlers for this module here.
        return services;
    }
}
