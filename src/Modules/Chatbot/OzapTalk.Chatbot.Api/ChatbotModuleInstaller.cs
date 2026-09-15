using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OzapTalk.Chatbot.Application;
using OzapTalk.Chatbot.Infrastructure;
using OzapTalk.SharedKernel.Modules;

namespace OzapTalk.Chatbot.Api;

public sealed class ChatbotModuleInstaller : IModuleInstaller
{
    public string ModuleName => "Chatbot";

    public IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddChatbotApplication();
        services.AddChatbotInfrastructure(configuration);
        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapChatbotEndpoints();
}
