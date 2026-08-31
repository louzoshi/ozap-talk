using SopaTalk.Accounts.Application.Abstractions;
using SopaTalk.SharedKernel.Results;

namespace SopaTalk.Accounts.Application.Team;

public sealed class ListMembersHandler(IUserRepository users)
{
    public async Task<Result<IReadOnlyList<MemberSummary>>> HandleAsync(CancellationToken ct)
    {
        var members = await users.ListByTenantAsync(ct);
        IReadOnlyList<MemberSummary> result =
        [
            .. members
                .OrderBy(u => u.Role)
                .ThenBy(u => u.DisplayName)
                .Select(u => new MemberSummary(
                    u.Id, u.Email, u.DisplayName, u.Role, u.IsActive, u.CreatedAtUtc, u.LastSignedInAtUtc)),
        ];
        return Result.Success(result);
    }
}
