using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OzapTalk.AiAgent.Application;
using OzapTalk.AiAgent.Infrastructure;
using OzapTalk.SharedKernel.Modules;

namespace OzapTalk.AiAgent.Api;

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
