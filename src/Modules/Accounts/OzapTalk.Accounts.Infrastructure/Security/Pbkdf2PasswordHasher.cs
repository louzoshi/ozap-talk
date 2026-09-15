using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OzapTalk.Accounts.Application.Abstractions;

namespace OzapTalk.Accounts.Infrastructure.Security;

/// <summary>
/// Wraps ASP.NET Core Identity's PBKDF2 hasher (v3 format: HMAC-SHA512, per-user salt,
/// high iteration count) behind the module's own interface.
/// </summary>
internal sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private static readonly object Marker = new();
    private readonly PasswordHasher<object> _inner =
        new(Options.Create(new PasswordHasherOptions { IterationCount = 210_000 }));

    public string Hash(string password) => _inner.HashPassword(Marker, password);

    public bool Verify(string storedHash, string providedPassword)
    {
        try
        {
            var result = _inner.VerifyHashedPassword(Marker, storedHash, providedPassword);
            return result is PasswordVerificationResult.Success
                or PasswordVerificationResult.SuccessRehashNeeded;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
