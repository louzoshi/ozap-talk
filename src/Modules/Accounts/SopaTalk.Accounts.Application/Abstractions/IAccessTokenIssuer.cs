using SopaTalk.Accounts.Domain.Accounts;
using SopaTalk.Accounts.Domain.Users;

namespace SopaTalk.Accounts.Application.Abstractions;

public sealed record IssuedToken(string AccessToken, DateTimeOffset ExpiresAtUtc);

public interface IAccessTokenIssuer
{
    /// <summary>
    /// Issues a signed JWT carrying: sub (user id), tenant_id (account id), email,
    /// name, and role. The API's JWT bearer config validates it.
    /// </summary>
    IssuedToken Issue(User user, Account account);
}
