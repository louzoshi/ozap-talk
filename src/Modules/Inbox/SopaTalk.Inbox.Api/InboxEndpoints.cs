using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace SopaTalk.Inbox.Api;

internal static class InboxEndpoints
{
    public static void MapInboxEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/inbox")
            .WithTags("Inbox")
            .RequireAuthorization();

        group.MapGet("/_ping", () => Results.Ok(new { module = "Inbox", status = "ok" }))
            .WithName("Inbox Ping")
            .AllowAnonymous();

        // TODO: map this module's real endpoints (one file per feature slice is fine).
    }
}
