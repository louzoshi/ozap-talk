using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SopaTalk.Inbox.Application;
using SopaTalk.Inbox.Infrastructure;
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
}
