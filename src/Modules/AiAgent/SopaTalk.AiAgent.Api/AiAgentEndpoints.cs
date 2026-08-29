using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace SopaTalk.AiAgent.Api;

internal static class AiAgentEndpoints
{
    public static void MapAiAgentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/ai-agent")
            .WithTags("AiAgent")
            .RequireAuthorization();

        group.MapGet("/_ping", () => Results.Ok(new { module = "AiAgent", status = "ok" }))
            .WithName("AiAgent Ping")
            .AllowAnonymous();

        // TODO: map this module's real endpoints (one file per feature slice is fine).
    }
}
