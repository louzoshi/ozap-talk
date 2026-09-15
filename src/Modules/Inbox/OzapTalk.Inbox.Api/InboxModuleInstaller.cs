using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OzapTalk.Inbox.Application;
using OzapTalk.Inbox.Infrastructure;
using OzapTalk.Inbox.Infrastructure.Persistence;
using OzapTalk.SharedKernel.Modules;

namespace OzapTalk.Inbox.Api;

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
