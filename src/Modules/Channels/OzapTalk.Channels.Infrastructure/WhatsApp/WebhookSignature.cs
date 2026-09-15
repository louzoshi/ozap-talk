using System.Security.Cryptography;
using System.Text;

namespace OzapTalk.Channels.Infrastructure.WhatsApp;

/// <summary>Validates the <c>X-Hub-Signature-256</c> header Meta sends with every webhook POST.</summary>
public static class WebhookSignature
{
    public static bool IsValid(string appSecret, string? signatureHeader, ReadOnlySpan<byte> rawBody)
    {
        if (string.IsNullOrEmpty(appSecret) || string.IsNullOrEmpty(signatureHeader))
            return false;

        const string prefix = "sha256=";
        if (!signatureHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        Span<byte> computed = stackalloc byte[32];
        HMACSHA256.HashData(Encoding.UTF8.GetBytes(appSecret), rawBody, computed);

        var expected = Convert.ToHexStringLower(computed);
        var provided = signatureHeader[prefix.Length..];

        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(expected), Encoding.ASCII.GetBytes(provided));
    }
}
