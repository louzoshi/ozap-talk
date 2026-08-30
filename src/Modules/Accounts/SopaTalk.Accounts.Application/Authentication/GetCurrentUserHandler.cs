using SopaTalk.Accounts.Application.Abstractions;
using SopaTalk.Accounts.Domain.Accounts;
using SopaTalk.SharedKernel.Results;

namespace SopaTalk.Accounts.Application.Authentication;

public sealed class GetCurrentUserHandler(IUserRepository users, IAccountRepository accounts)
{
    public async Task<Result<CurrentUser>> HandleAsync(Guid userId, CancellationToken ct)
    {
        var user = await users.GetAsync(userId, ct);
        if (user is null || !user.IsActive)
            return AuthenticationErrors.InvalidCredentials;

        var account = await accounts.GetAsync(user.TenantId, ct);
        if (account is null)
            return AccountErrors.NotFound;

        return new CurrentUser(
            user.Id, account.Id, user.Email, user.DisplayName, user.Role.ToString(), account.Name);
    }
}
