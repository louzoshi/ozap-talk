using Microsoft.Extensions.Logging;
using SopaTalk.Channels.Application.Abstractions;
using SopaTalk.Channels.Domain.WhatsApp;
using SopaTalk.SharedContracts.Channels;
using SopaTalk.SharedKernel.Messaging;
using SopaTalk.SharedKernel.MultiTenancy;

namespace SopaTalk.Channels.Application.Webhooks;

/// <summary>
/// Turns a validated WhatsApp webhook body into <see cref="InboundMessageReceived"/> events.
/// Runs with no tenant in scope: it resolves the tenant from the channel that owns the
/// <c>phone_number_id</c>, then sets it for the rest of the unit of work.
/// </summary>
public sealed class ProcessWebhookHandler(
    IChannelRepository channels,
    IProcessedMessageStore processed,
    IChannelsUnitOfWork unitOfWork,
    ISettableTenantContext tenantContext,
    IEventBus eventBus,
    ILogger<ProcessWebhookHandler> logger)
{
    public async Task HandleAsync(MetaWebhookPayload payload, CancellationToken ct)
    {
        foreach (var change in payload.Entry.SelectMany(e => e.Changes))
        {
            var value = change.Value;
            var phoneNumberId = value?.Metadata?.PhoneNumberId;
            if (value is null || string.IsNullOrEmpty(phoneNumberId) || value.Messages.Count == 0)
                continue;

            var channel = await channels.FindByPhoneNumberIdAsync(phoneNumberId, ct);
            if (channel is null || !channel.IsActive)
            {
                logger.LogWarning("Webhook for unknown or inactive phone_number_id {PhoneNumberId}", phoneNumberId);
                continue;
            }

            tenantContext.SetTenant(channel.TenantId);

            foreach (var message in value.Messages)
            {
                if (message.Type != "text" || message.Text is null)
                {
                    logger.LogInformation("Ignoring non-text message {MessageId} of type {Type}", message.Id, message.Type);
                    continue;
                }

                if (await processed.AlreadyProcessedAsync(channel.TenantId, message.Id, ct))
                    continue;

                var contact = value.Contacts.FirstOrDefault(c => c.WaId == message.From);

                processed.MarkProcessed(ProcessedInboundMessage.Of(channel.TenantId, message.Id));
                await unitOfWork.SaveChangesAsync(ct);

                await eventBus.PublishAsync(new InboundMessageReceived(
                    channel.TenantId,
                    channel.Id,
                    message.From,
                    contact?.Profile?.Name,
                    message.Id,
                    message.Text.Body,
                    FromUnixSeconds(message.Timestamp)), ct);
            }
        }
    }

    private static DateTimeOffset FromUnixSeconds(string value) =>
        long.TryParse(value, out var seconds)
            ? DateTimeOffset.FromUnixTimeSeconds(seconds)
            : DateTimeOffset.UtcNow;
}
