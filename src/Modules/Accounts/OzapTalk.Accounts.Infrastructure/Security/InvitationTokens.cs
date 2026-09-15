using System.Security.Cryptography;
using System.Text;
using OzapTalk.Accounts.Application.Abstractions;

namespace OzapTalk.Accounts.Infrastructure.Security;

/// <summary>
/// Token de convite: 32 bytes aleatórios em base64url no link; SHA-256 em hexadecimal
/// no banco. O valor cru nunca é persistido.
/// </summary>
internal sealed class InvitationTokens : IInvitationTokens
{
    public InvitationToken Create()
    {
        var raw = Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        return new InvitationToken(raw, Hash(raw));
    }

    public string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexStringLower(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
