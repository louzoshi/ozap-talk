using SopaTalk.Inbox.Application.Abstractions;
using SopaTalk.Inbox.Domain.Contacts;
using SopaTalk.Inbox.Domain.Conversations;
using SopaTalk.Inbox.Domain.Messages;
using SopaTalk.SharedContracts.Channels;
using SopaTalk.SharedContracts.Inbox;
using SopaTalk.SharedKernel.Messaging;

namespace SopaTalk.Inbox.Application.Messaging;

/// <summary>
/// Channels received a WhatsApp message. Upsert the contact, find or open the
/// conversation, append the inbound message, and announce the change.
/// </summary>
public sealed class InboundMessageReceivedHandler(
    IContactRepository contacts,
    IConversationRepository conversations,
    IMessageRepository messages,
    IInboxUnitOfWork unitOfWork,
    IEventBus eventBus)
    : IIntegrationEventHandler<InboundMessageReceived>
{
    public async Task HandleAsync(InboundMessageReceived e, CancellationToken ct = default)
    {
        var contact = await contacts.FindByPhoneAsync(e.ContactPhone, ct);
        if (contact is null)
        {
            contact = Contact.Create(e.TenantId, e.ContactPhone, e.ContactName);
            contacts.Add(contact);
        }
        else
        {
            contact.EnrichName(e.ContactName);
        }

        var conversation = await conversations.FindOpenAsync(e.ChannelId, contact.Id, ct);
        if (conversation is null)
        {
            conversation = Conversation.Start(e.TenantId, e.ChannelId, contact.Id);
            conversations.Add(conversation);
        }

        conversation.RecordInbound(e.Text, e.OccurredAtProvider);
        messages.Add(Message.Inbound(e.TenantId, conversation.Id, e.Text, e.ProviderMessageId));

        await unitOfWork.SaveChangesAsync(ct);
        await eventBus.PublishAsync(new ConversationChanged(e.TenantId, conversation.Id), ct);
    }
}
