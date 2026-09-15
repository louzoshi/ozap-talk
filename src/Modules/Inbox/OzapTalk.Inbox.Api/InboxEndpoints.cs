using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OzapTalk.Inbox.Application.Conversations;
using OzapTalk.Inbox.Application.Queries;
using OzapTalk.SharedKernel.Http;

namespace OzapTalk.Inbox.Api;

internal static class InboxEndpoints
{
    public static void MapInboxEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/inbox").WithTags("Inbox").RequireAuthorization();

        group.MapGet("/conversations", async (string? status, IInboxQueries queries, CancellationToken ct) =>
            Results.Ok(await queries.ListConversationsAsync(status, ct)))
            .WithSummary("List conversations (status: open | waiting | closed)");

        group.MapGet("/conversations/{id:guid}", async (Guid id, IInboxQueries queries, CancellationToken ct) =>
        {
            var thread = await queries.GetThreadAsync(id, ct);
            return thread is null ? Results.NotFound() : Results.Ok(thread);
        }).WithSummary("Get a conversation with its messages");

        group.MapPost("/conversations/{id:guid}/messages", async (
            Guid id, SendReplyBody body, ClaimsPrincipal user, SendReplyHandler handler, CancellationToken ct) =>
        {
            if (!TryGetUserId(user, out var agentId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(new SendReplyCommand(id, body.Text, agentId), ct);
            return result.Match(messageId => Results.Accepted($"/api/inbox/messages/{messageId}", new { messageId }));
        }).RequireAuthorization("operator").WithSummary("Send a reply in a conversation");

        group.MapPost("/conversations/{id:guid}/assign", async (
            Guid id, AssignBody body, AssignConversationHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new AssignConversationCommand(id, body.AgentId), ct);
            return result.Match(Results.NoContent);
        }).RequireAuthorization("operator").WithSummary("Assign a conversation to an agent");

        group.MapPost("/conversations/{id:guid}/close", async (
            Guid id, CloseConversationHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new CloseConversationCommand(id), ct);
            return result.Match(Results.NoContent);
        }).RequireAuthorization("operator").WithSummary("Close a conversation");

        group.MapPost("/conversations/{id:guid}/read", async (
            Guid id, MarkConversationReadHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new MarkConversationReadCommand(id), ct);
            return result.Match(Results.NoContent);
        }).RequireAuthorization("operator").WithSummary("Reset the unread counter");
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out Guid userId) =>
        Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub"), out userId);

    private sealed record SendReplyBody(string Text);
    private sealed record AssignBody(Guid AgentId);
}
