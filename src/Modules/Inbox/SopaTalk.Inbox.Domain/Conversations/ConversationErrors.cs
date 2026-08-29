using SopaTalk.SharedKernel.Results;

namespace SopaTalk.Inbox.Domain.Conversations;

public static class ConversationErrors
{
    public static readonly Error AlreadyClosed =
        Error.Conflict("conversation.already_closed", "A conversa já está encerrada.");

    public static readonly Error NotClosed =
        Error.Conflict("conversation.not_closed", "A conversa não está encerrada.");

    public static readonly Error ClosedCannotBeAssigned =
        Error.Conflict("conversation.closed_cannot_be_assigned", "Não é possível atribuir uma conversa encerrada.");
}
