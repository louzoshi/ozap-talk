using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SopaTalk.Accounts.Application.Authentication;

namespace SopaTalk.Accounts.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAccountsApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

        services.AddScoped<RegisterAccountHandler>();
        services.AddScoped<AuthenticateHandler>();
        services.AddScoped<GetCurrentUserHandler>();

        return services;
    }
}
