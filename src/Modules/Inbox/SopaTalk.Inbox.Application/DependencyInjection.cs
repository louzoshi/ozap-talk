using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SopaTalk.Inbox.Application.Conversations;
using SopaTalk.Inbox.Application.Messaging;
using SopaTalk.SharedContracts.Channels;
using SopaTalk.SharedKernel.Messaging;

namespace SopaTalk.Inbox.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInboxApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

        services.AddScoped<SendReplyHandler>();
        services.AddScoped<AssignConversationHandler>();
        services.AddScoped<CloseConversationHandler>();
        services.AddScoped<MarkConversationReadHandler>();

        services.AddScoped<IIntegrationEventHandler<InboundMessageReceived>, InboundMessageReceivedHandler>();
        services.AddScoped<IIntegrationEventHandler<OutboundMessageDispatched>, OutboundMessageDispatchedHandler>();
        services.AddScoped<IIntegrationEventHandler<OutboundMessageFailed>, OutboundMessageFailedHandler>();

        return services;
    }
}
