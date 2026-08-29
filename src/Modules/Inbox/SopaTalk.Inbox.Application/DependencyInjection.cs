using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace SopaTalk.Inbox.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInboxApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
        // TODO: register command/query dispatcher handlers for this module here.
        return services;
    }
}
