using System.ComponentModel.DataAnnotations;

namespace OzapTalk.Accounts.Infrastructure.Security;

/// <summary>Bound from the <c>Jwt</c> configuration section. The signing key must come
/// from a secret store (user-secrets in dev, env var / vault in prod), never a
/// committed appsettings file.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required] public string Issuer { get; init; } = "ozap-talk";
    [Required] public string Audience { get; init; } = "ozap-talk-api";

    [Required, MinLength(32)]
    public string SigningKey { get; init; } = string.Empty;

    [Range(5, 1440)]
    public int AccessTokenMinutes { get; init; } = 60;
}
