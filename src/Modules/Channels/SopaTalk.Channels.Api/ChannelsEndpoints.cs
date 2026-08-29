using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace SopaTalk.Channels.Api;

internal static class ChannelsEndpoints
{
    public static void MapChannelsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/channels")
            .WithTags("Channels")
            .RequireAuthorization();

        group.MapGet("/_ping", () => Results.Ok(new { module = "Channels", status = "ok" }))
            .WithName("Channels Ping")
            .AllowAnonymous();

        // TODO: map this module's real endpoints (one file per feature slice is fine).
    }
}
