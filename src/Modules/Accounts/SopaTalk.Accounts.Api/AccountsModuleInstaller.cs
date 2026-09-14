using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SopaTalk.Accounts.Application;
using SopaTalk.Accounts.Infrastructure;
using SopaTalk.Accounts.Infrastructure.Jobs;
using SopaTalk.Accounts.Infrastructure.Persistence;
using SopaTalk.SharedKernel.Jobs;
using SopaTalk.SharedKernel.Modules;

namespace SopaTalk.Accounts.Api;

public sealed class AccountsModuleInstaller : IModuleInstaller
{
    /// <summary>Toda segunda-feira às 12:00 UTC (9:00 em Brasília).</summary>
    private const string WeeklyDigestCron = "0 12 * * 1";

    public string ModuleName => "Accounts";

    public IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAccountsApplication();
        services.AddAccountsInfrastructure(configuration);
        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAccountsEndpoints();
        endpoints.MapTeamEndpoints();
    }

    public void RegisterRecurringJobs(IRecurringJobRegistry jobs) =>
        jobs.Schedule<WeeklyDigestJob>(
            "accounts:weekly-digest", WeeklyDigestCron, job => job.RunAsync(CancellationToken.None));

    public async Task MigrateAsync(IServiceProvider services, CancellationToken ct)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AccountsDbContext>().Database.MigrateAsync(ct);
    }
}
