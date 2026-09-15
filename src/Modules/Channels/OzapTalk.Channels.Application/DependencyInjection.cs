using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OzapTalk.Channels.Application.Channels;
using OzapTalk.Channels.Application.Sending;
using OzapTalk.Channels.Application.Webhooks;
using OzapTalk.SharedContracts.Inbox;
using OzapTalk.SharedKernel.Messaging;

namespace OzapTalk.Channels.Application;

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
