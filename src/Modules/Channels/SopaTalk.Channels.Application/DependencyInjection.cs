using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SopaTalk.Channels.Application.Channels;
using SopaTalk.Channels.Application.Sending;
using SopaTalk.Channels.Application.Webhooks;
using SopaTalk.SharedContracts.Inbox;
using SopaTalk.SharedKernel.Messaging;

namespace SopaTalk.Channels.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddChannelsApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

        services.AddScoped<ProcessWebhookHandler>();
        services.AddScoped<ConnectChannelHandler>();
        services.AddScoped<ListChannelsHandler>();

        services.AddScoped<IIntegrationEventHandler<OutboundMessageRequested>, OutboundMessageRequestedHandler>();

        return services;
    }
}
