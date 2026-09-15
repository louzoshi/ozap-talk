using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OzapTalk.Accounts.Application;
using OzapTalk.Accounts.Infrastructure;
using OzapTalk.Accounts.Infrastructure.Jobs;
using OzapTalk.Accounts.Infrastructure.Persistence;
using OzapTalk.SharedKernel.Jobs;
using OzapTalk.SharedKernel.Modules;

namespace OzapTalk.Accounts.Api;

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
