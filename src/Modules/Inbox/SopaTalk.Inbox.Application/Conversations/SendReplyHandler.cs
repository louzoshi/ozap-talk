using FluentValidation;
using SopaTalk.Inbox.Application.Abstractions;
using SopaTalk.Inbox.Domain.Messages;
using SopaTalk.SharedContracts.Inbox;
using SopaTalk.SharedKernel.Messaging;
using SopaTalk.SharedKernel.Results;

namespace SopaTalk.Inbox.Application.Conversations;

public sealed record SendReplyCommand(Guid ConversationId, string Text, Guid AuthorAgentId);

public sealed class SendReplyCommandValidator : AbstractValidator<SendReplyCommand>
{
    public SendReplyCommandValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(4096);
    }
}

public static class ConversationCommandErrors
{
    public static readonly Error NotFound =
        Error.NotFound("conversation.not_found", "Conversa não encontrada.");

    public static readonly Error Closed =
        Error.Conflict("conversation.closed", "A conversa está encerrada.");
}

public sealed class SendReplyHandler(
    IConversationRepository conversations,
    IContactRepository contacts,
    IMessageRepository messages,
    IInboxUnitOfWork unitOfWork,
    IEventBus eventBus,
    IValidator<SendReplyCommand> validator)
{
    public async Task<Result<Guid>> HandleAsync(SendReplyCommand command, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return Error.Validation("reply.invalid", validation.Errors[0].ErrorMessage);

        var conversation = await conversations.GetAsync(command.ConversationId, ct);
        if (conversation is null)
            return ConversationCommandErrors.NotFound;
        if (conversation.Status == Domain.Conversations.ConversationStatus.Closed)
            return ConversationCommandErrors.Closed;

        var contact = await contacts.GetAsync(conversation.ContactId, ct);
        if (contact is null)
            return ConversationCommandErrors.NotFound;

        var message = Message.QueuedReply(conversation.TenantId, conversation.Id, command.Text, command.AuthorAgentId);
        messages.Add(message);
        conversation.RecordOutbound(command.Text, DateTimeOffset.UtcNow);

        await unitOfWork.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new OutboundMessageRequested(
            conversation.TenantId, conversation.ChannelId, message.Id, contact.Phone, command.Text), ct);
        await eventBus.PublishAsync(new ConversationChanged(conversation.TenantId, conversation.Id), ct);

        return message.Id;
    }
}
