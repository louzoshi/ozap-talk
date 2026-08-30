using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SopaTalk.Inbox.Application;
using SopaTalk.Inbox.Infrastructure;
using SopaTalk.Inbox.Infrastructure.Persistence;
using SopaTalk.SharedKernel.Modules;

namespace SopaTalk.Inbox.Api;

public sealed class InboxModuleInstaller : IModuleInstaller
{
    public string ModuleName => "Inbox";

    public IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddInboxApplication();
        services.AddInboxInfrastructure(configuration);
        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapInboxEndpoints();

    public async Task MigrateAsync(IServiceProvider services, CancellationToken ct)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<InboxDbContext>().Database.MigrateAsync(ct);
    }
}
