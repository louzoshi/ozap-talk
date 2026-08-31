using SopaTalk.Accounts.Application.Abstractions;
using SopaTalk.SharedKernel.Results;

namespace SopaTalk.Accounts.Application.Team;

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
