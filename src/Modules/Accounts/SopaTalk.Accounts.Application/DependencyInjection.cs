using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SopaTalk.Accounts.Application.Authentication;
using SopaTalk.Accounts.Application.Team;

namespace SopaTalk.Accounts.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAccountsApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

        services.AddScoped<RegisterAccountHandler>();
        services.AddScoped<AuthenticateHandler>();
        services.AddScoped<GetCurrentUserHandler>();

        services.AddScoped<InviteMemberHandler>();
        services.AddScoped<ListInvitationsHandler>();
        services.AddScoped<RevokeInvitationHandler>();
        services.AddScoped<GetInvitationHandler>();
        services.AddScoped<AcceptInvitationHandler>();
        services.AddScoped<ListMembersHandler>();
        services.AddScoped<ChangeMemberRoleHandler>();
        services.AddScoped<DeactivateMemberHandler>();

        return services;
    }
}
