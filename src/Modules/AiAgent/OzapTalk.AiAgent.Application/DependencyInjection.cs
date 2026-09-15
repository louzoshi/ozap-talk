using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace OzapTalk.AiAgent.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAiAgentApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
        // TODO: register command/query dispatcher handlers for this module here.
        return services;
    }
}
