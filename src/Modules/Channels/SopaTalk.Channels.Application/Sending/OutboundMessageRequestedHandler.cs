using Microsoft.Extensions.Logging;
using SopaTalk.Channels.Application.Abstractions;
using SopaTalk.SharedContracts.Channels;
using SopaTalk.SharedContracts.Inbox;
using SopaTalk.SharedKernel.Messaging;

namespace SopaTalk.Channels.Application.Sending;

/// <summary>
/// Inbox asked for a reply to go out. Send it through the WhatsApp API and report back
/// with <see cref="OutboundMessageDispatched"/> or <see cref="OutboundMessageFailed"/>.
/// </summary>
public sealed class OutboundMessageRequestedHandler(
    IChannelRepository channels,
    IWhatsAppApi whatsApp,
    IEventBus eventBus,
    ILogger<OutboundMessageRequestedHandler> logger)
    : IIntegrationEventHandler<OutboundMessageRequested>
{
    public async Task HandleAsync(OutboundMessageRequested e, CancellationToken ct = default)
    {
        var channel = await channels.GetAsync(e.ChannelId, ct);
        if (channel is null || channel.TenantId != e.TenantId || !channel.IsActive)
        {
            await eventBus.PublishAsync(
                new OutboundMessageFailed(e.TenantId, e.MessageId, "Canal não encontrado ou inativo."), ct);
            return;
        }

        try
        {
            var providerMessageId = await whatsApp.SendTextAsync(
                channel.PhoneNumberId, channel.AccessToken, e.ToPhone, e.Text, ct);

            await eventBus.PublishAsync(
                new OutboundMessageDispatched(e.TenantId, e.MessageId, providerMessageId), ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send WhatsApp message {MessageId}", e.MessageId);
            await eventBus.PublishAsync(
                new OutboundMessageFailed(e.TenantId, e.MessageId, ex.Message), ct);
        }
    }
}
