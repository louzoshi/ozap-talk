using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SopaTalk.Accounts.Application;
using SopaTalk.Accounts.Infrastructure;
using SopaTalk.Accounts.Infrastructure.Persistence;
using SopaTalk.SharedKernel.Modules;

namespace SopaTalk.Accounts.Api;

public sealed class AccountsModuleInstaller : IModuleInstaller
{
    public string ModuleName => "Accounts";

    public IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAccountsApplication();
        services.AddAccountsInfrastructure(configuration);
        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapAccountsEndpoints();

    public async Task MigrateAsync(IServiceProvider services, CancellationToken ct)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AccountsDbContext>().Database.MigrateAsync(ct);
    }
}
