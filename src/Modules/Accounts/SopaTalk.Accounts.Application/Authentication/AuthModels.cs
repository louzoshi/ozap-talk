namespace SopaTalk.Accounts.Application.Authentication;

public sealed record CurrentUser(
    Guid Id,
    Guid AccountId,
    string Email,
    string DisplayName,
    string Role,
    string CompanyName);

public sealed record AuthenticationResult(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    CurrentUser User);
