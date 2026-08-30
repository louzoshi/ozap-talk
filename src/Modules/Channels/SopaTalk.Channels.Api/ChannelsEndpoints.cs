using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SopaTalk.Channels.Application.Channels;
using SopaTalk.Channels.Application.Webhooks;
using SopaTalk.Channels.Infrastructure.WhatsApp;
using SopaTalk.SharedKernel.Http;

namespace SopaTalk.Channels.Api;

internal static class ChannelsEndpoints
{
    public static void MapChannelsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/channels").WithTags("Channels");

        // --- Meta webhook (called by Meta, not by our users) ---
        group.MapGet("/webhook", (HttpRequest request, IOptions<ChannelsOptions> options) =>
        {
            var mode = request.Query["hub.mode"];
            var token = request.Query["hub.verify_token"];
            var challenge = request.Query["hub.challenge"].ToString();

            return mode == "subscribe" && token == options.Value.WebhookVerifyToken
                ? Results.Text(challenge)
                : Results.Forbid();
        }).AllowAnonymous().WithSummary("Meta webhook verification handshake");

        group.MapPost("/webhook", async (
            HttpRequest request,
            IOptions<ChannelsOptions> options,
            ProcessWebhookHandler handler,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            using var buffer = new MemoryStream();
            await request.Body.CopyToAsync(buffer, ct);
            var raw = buffer.ToArray();

            var signature = request.Headers["X-Hub-Signature-256"].ToString();
            if (!WebhookSignature.IsValid(options.Value.AppSecret, signature, raw))
            {
                loggerFactory.CreateLogger("Channels.Webhook").LogWarning("Rejected webhook with bad signature");
                return Results.Unauthorized();
            }

            var payload = JsonSerializer.Deserialize<MetaWebhookPayload>(raw);
            if (payload is not null)
                await handler.HandleAsync(payload, ct);

            return Results.Ok(); // Meta expects a fast 200 regardless of downstream outcome
        }).AllowAnonymous().WithSummary("Meta webhook receiver");

        // --- Tenant-facing channel management ---
        group.MapPost("/", async (ConnectChannelCommand command, ConnectChannelHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);
            return result.Match(summary => Results.Created($"/api/channels/{summary.Id}", summary));
        }).RequireAuthorization("admin").WithSummary("Connect a WhatsApp number");

        group.MapGet("/", async (ListChannelsHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(ct)))
            .RequireAuthorization().WithSummary("List connected WhatsApp numbers");
    }
}
