using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OzapTalk.Crm.Application;
using OzapTalk.Crm.Infrastructure;
using OzapTalk.SharedKernel.Modules;

namespace OzapTalk.Crm.Api;

public sealed class CrmModuleInstaller : IModuleInstaller
{
    public string ModuleName => "Crm";

    public IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCrmApplication();
        services.AddCrmInfrastructure(configuration);
        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapCrmEndpoints();
}
