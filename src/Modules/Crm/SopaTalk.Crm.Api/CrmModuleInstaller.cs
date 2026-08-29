using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SopaTalk.Crm.Application;
using SopaTalk.Crm.Infrastructure;
using SopaTalk.SharedKernel.Modules;

namespace SopaTalk.Crm.Api;

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
