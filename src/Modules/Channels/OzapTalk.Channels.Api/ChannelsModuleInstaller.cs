using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OzapTalk.Channels.Application;
using OzapTalk.Channels.Infrastructure;
using OzapTalk.Channels.Infrastructure.Persistence;
using OzapTalk.SharedKernel.Modules;

namespace OzapTalk.Channels.Api;

public sealed class ChannelsModuleInstaller : IModuleInstaller
{
    public string ModuleName => "Channels";

    public IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddChannelsApplication();
        services.AddChannelsInfrastructure(configuration);
        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapChannelsEndpoints();

    public async Task MigrateAsync(IServiceProvider services, CancellationToken ct)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ChannelsDbContext>().Database.MigrateAsync(ct);
    }
}
