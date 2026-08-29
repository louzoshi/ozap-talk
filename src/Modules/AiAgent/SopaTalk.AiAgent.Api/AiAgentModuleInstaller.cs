using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SopaTalk.AiAgent.Application;
using SopaTalk.AiAgent.Infrastructure;
using SopaTalk.SharedKernel.Modules;

namespace SopaTalk.AiAgent.Api;

public sealed class AiAgentModuleInstaller : IModuleInstaller
{
    public string ModuleName => "AiAgent";

    public IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAiAgentApplication();
        services.AddAiAgentInfrastructure(configuration);
        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapAiAgentEndpoints();
}
