using OzapTalk.Accounts.Application.Abstractions;
using OzapTalk.SharedKernel.Results;

namespace OzapTalk.Accounts.Application.Team;

public sealed class ListInvitationsHandler(IInvitationRepository invitations)
{
    public async Task<Result<IReadOnlyList<InvitationSummary>>> HandleAsync(CancellationToken ct)
    {
        var pending = await invitations.ListPendingAsync(ct);
        IReadOnlyList<InvitationSummary> result =
            [.. pending.Select(InviteMemberHandler.ToSummary)];
        return Result.Success(result);
    }
}
