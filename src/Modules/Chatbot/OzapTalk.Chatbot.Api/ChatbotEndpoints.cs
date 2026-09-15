using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace OzapTalk.Chatbot.Api;

internal static class ChatbotEndpoints
{
    public static void MapChatbotEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/chatbot")
            .WithTags("Chatbot")
            .RequireAuthorization();

        group.MapGet("/_ping", () => Results.Ok(new { module = "Chatbot", status = "ok" }))
            .WithName("Chatbot Ping")
            .AllowAnonymous();

        // TODO: map this module's real endpoints (one file per feature slice is fine).
    }
}
