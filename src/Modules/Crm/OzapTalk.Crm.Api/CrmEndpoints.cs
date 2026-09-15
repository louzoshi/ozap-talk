using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace OzapTalk.Crm.Api;

internal static class CrmEndpoints
{
    public static void MapCrmEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/crm")
            .WithTags("Crm")
            .RequireAuthorization();

        group.MapGet("/_ping", () => Results.Ok(new { module = "Crm", status = "ok" }))
            .WithName("Crm Ping")
            .AllowAnonymous();

        // TODO: map this module's real endpoints (one file per feature slice is fine).
    }
}
