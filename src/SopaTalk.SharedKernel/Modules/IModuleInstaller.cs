using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SopaTalk.SharedKernel.Modules;

/// <summary>
/// Each module ships one installer. The host discovers all installers by reflection and calls them.
/// The host never references a module's internals — only its *.Api and *.Infrastructure assemblies
/// and this contract. This is what keeps the monolith modular.
/// </summary>
public interface IModuleInstaller
{
    string ModuleName { get; }

    /// <summary>Register the module's services, DbContext, validators and event handlers.</summary>
    IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration);

    /// <summary>Map the module's HTTP endpoints under its own route group.</summary>
    void MapEndpoints(IEndpointRouteBuilder endpoints);

    /// <summary>
    /// Apply the module's EF Core migrations. Called by the host on startup in Development,
    /// and in any environment where Database:MigrateOnStartup is enabled (single-instance
    /// deploys). Otherwise migrations run as a separate deploy step. No-op by default.
    /// </summary>
    Task MigrateAsync(IServiceProvider services, CancellationToken ct) => Task.CompletedTask;
}
