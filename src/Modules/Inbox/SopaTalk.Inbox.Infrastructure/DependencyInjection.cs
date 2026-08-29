using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SopaTalk.Inbox.Infrastructure.Persistence;

namespace SopaTalk.Inbox.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInboxInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("ConnectionStrings:Database is not configured.");

        services.AddDbContext<InboxDbContext>(options => options
            .UseNpgsql(connectionString, npgsql => npgsql
                .MigrationsHistoryTable("__ef_migrations_history", InboxDbContext.Schema)));

        // TODO: register repositories and integration-event handlers for this module here.
        return services;
    }
}
