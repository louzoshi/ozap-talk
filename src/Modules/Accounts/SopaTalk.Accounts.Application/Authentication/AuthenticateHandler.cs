using SopaTalk.Accounts.Application.Abstractions;
using SopaTalk.Accounts.Domain.Users;
using SopaTalk.SharedKernel.Results;

namespace SopaTalk.Accounts.Application.Authentication;

public sealed record AuthenticateCommand(string Email, string Password);

public static class AuthenticationErrors
{
    // One opaque error for "bad email" and "bad password" — never reveal which.
    public static readonly Error InvalidCredentials =
        Error.Validation("auth.invalid_credentials", "E-mail ou senha incorretos.");

    public static readonly Error AccountInactive =
        Error.Validation("auth.account_inactive", "Esta conta está inativa.");
}

public sealed class AuthenticateHandler(
    IUserRepository users,
    IAccountRepository accounts,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer tokenIssuer,
    IAccountsUnitOfWork unitOfWork)
{
    // A real hash, computed once, used on the missing-user path so verification does
    // the same work whether or not the e-mail exists.
    private static string? _timingEqualizerHash;

    public async Task<Result<AuthenticationResult>> HandleAsync(AuthenticateCommand command, CancellationToken ct)
    {
        var email = User.Normalize(command.Email ?? string.Empty);
        var user = await users.FindByEmailAsync(email, ct);

        var hashToCheck = user?.PasswordHash
            ?? (_timingEqualizerHash ??= passwordHasher.Hash("timing-equalizer"));
        var passwordOk = passwordHasher.Verify(hashToCheck, command.Password ?? string.Empty);

        if (user is null || !passwordOk)
            return AuthenticationErrors.InvalidCredentials;

        if (!user.IsActive)
            return AuthenticationErrors.AccountInactive;

        var account = await accounts.GetAsync(user.TenantId, ct);
        if (account is null || !account.IsActive)
            return AuthenticationErrors.AccountInactive;

        user.MarkSignedIn();
        await unitOfWork.SaveChangesAsync(ct);

        var token = tokenIssuer.Issue(user, account);
        return new AuthenticationResult(
            token.AccessToken,
            token.ExpiresAtUtc,
            new CurrentUser(user.Id, account.Id, user.Email, user.DisplayName, user.Role.ToString(), account.Name));
    }
}
