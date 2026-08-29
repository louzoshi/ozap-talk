using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SopaTalk.Channels.Infrastructure.Persistence;

namespace SopaTalk.Channels.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddChannelsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("ConnectionStrings:Database is not configured.");

        services.AddDbContext<ChannelsDbContext>(options => options
            .UseNpgsql(connectionString, npgsql => npgsql
                .MigrationsHistoryTable("__ef_migrations_history", ChannelsDbContext.Schema)));

        // TODO: register repositories and integration-event handlers for this module here.
        return services;
    }
}
