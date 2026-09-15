using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OzapTalk.Channels.Application.Abstractions;
using OzapTalk.Channels.Infrastructure.Persistence;
using OzapTalk.Channels.Infrastructure.WhatsApp;

namespace OzapTalk.Channels.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddChannelsInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("ConnectionStrings:Database is not configured.");

        services.AddDbContext<ChannelsDbContext>(options => options
            .UseNpgsql(connectionString, npgsql => npgsql
                .MigrationsHistoryTable("__ef_migrations_history", ChannelsDbContext.Schema)));

        services.AddScoped<IChannelRepository, ChannelRepository>();
        services.AddScoped<IProcessedMessageStore, ProcessedMessageStore>();
        services.AddScoped<IChannelsUnitOfWork>(sp => sp.GetRequiredService<ChannelsDbContext>());

        services.AddOptions<ChannelsOptions>()
            .Bind(configuration.GetSection(ChannelsOptions.SectionName));

        services.AddHttpClient<IWhatsAppApi, WhatsAppCloudApi>(client =>
            client.BaseAddress = new Uri("https://graph.facebook.com"));

        return services;
    }
}
