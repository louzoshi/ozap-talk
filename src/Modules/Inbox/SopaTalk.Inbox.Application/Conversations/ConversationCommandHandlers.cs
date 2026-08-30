using SopaTalk.Inbox.Application.Abstractions;
using SopaTalk.SharedContracts.Inbox;
using SopaTalk.SharedKernel.Messaging;
using SopaTalk.SharedKernel.Results;

namespace SopaTalk.Inbox.Application.Conversations;

public sealed record AssignConversationCommand(Guid ConversationId, Guid AgentId);
public sealed record CloseConversationCommand(Guid ConversationId);
public sealed record MarkConversationReadCommand(Guid ConversationId);

public sealed class AssignConversationHandler(
    IConversationRepository conversations, IInboxUnitOfWork unitOfWork, IEventBus eventBus)
{
    public async Task<Result> HandleAsync(AssignConversationCommand command, CancellationToken ct)
    {
        var conversation = await conversations.GetAsync(command.ConversationId, ct);
        if (conversation is null)
            return ConversationCommandErrors.NotFound;

        var result = conversation.AssignTo(command.AgentId);
        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(ct);
        await eventBus.PublishAsync(new ConversationChanged(conversation.TenantId, conversation.Id), ct);
        return Result.Success();
    }
}

public sealed class CloseConversationHandler(
    IConversationRepository conversations, IInboxUnitOfWork unitOfWork, IEventBus eventBus)
{
    public async Task<Result> HandleAsync(CloseConversationCommand command, CancellationToken ct)
    {
        var conversation = await conversations.GetAsync(command.ConversationId, ct);
        if (conversation is null)
            return ConversationCommandErrors.NotFound;

        var result = conversation.Close();
        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(ct);
        await eventBus.PublishAsync(new ConversationChanged(conversation.TenantId, conversation.Id), ct);
        return Result.Success();
    }
}

public sealed class MarkConversationReadHandler(
    IConversationRepository conversations, IInboxUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(MarkConversationReadCommand command, CancellationToken ct)
    {
        var conversation = await conversations.GetAsync(command.ConversationId, ct);
        if (conversation is null)
            return ConversationCommandErrors.NotFound;

        conversation.MarkRead();
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
