using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OzapTalk.Inbox.Application.Abstractions;
using OzapTalk.Inbox.Application.Queries;
using OzapTalk.Inbox.Infrastructure.Persistence;

namespace OzapTalk.Inbox.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInboxInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("ConnectionStrings:Database is not configured.");

        services.AddDbContext<InboxDbContext>(options => options
            .UseNpgsql(connectionString, npgsql => npgsql
                .MigrationsHistoryTable("__ef_migrations_history", InboxDbContext.Schema)));

        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IInboxQueries, InboxQueries>();
        services.AddScoped<IInboxUnitOfWork>(sp => sp.GetRequiredService<InboxDbContext>());

        return services;
    }
}
