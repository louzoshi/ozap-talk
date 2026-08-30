namespace SopaTalk.Accounts.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);

    /// <summary>Constant-time verification. Returns false for any malformed stored hash.</summary>
    bool Verify(string storedHash, string providedPassword);
}
