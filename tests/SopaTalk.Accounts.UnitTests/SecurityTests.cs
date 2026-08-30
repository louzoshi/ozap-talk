using FluentAssertions;
using Microsoft.Extensions.Options;
using SopaTalk.Accounts.Domain.Accounts;
using SopaTalk.Accounts.Domain.Users;
using SopaTalk.Accounts.Infrastructure.Security;

namespace SopaTalk.Accounts.UnitTests;

public class SecurityTests
{
    private static readonly Pbkdf2PasswordHasher Hasher = new();

    [Fact]
    public void Password_hash_round_trips()
    {
        var hash = Hasher.Hash("correct horse battery staple");

        hash.Should().NotBe("correct horse battery staple");
        Hasher.Verify(hash, "correct horse battery staple").Should().BeTrue();
        Hasher.Verify(hash, "wrong password").Should().BeFalse();
    }

    [Fact]
    public void Verify_returns_false_for_garbage_hash()
    {
        Hasher.Verify("not-a-real-hash", "anything").Should().BeFalse();
    }

    [Fact]
    public void Issued_token_carries_sub_tenant_and_role_claims()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "sopa-talk",
            Audience = "sopa-talk-api",
            SigningKey = "unit-test-signing-key-that-is-long-enough-32+",
            AccessTokenMinutes = 30,
        });
        var issuer = new JwtAccessTokenIssuer(options);

        var (account, owner) = Account.Register("Acme", "acme", "o@acme.com", "Owner", "hash").Value;
        var token = issuer.Issue(owner, account);

        token.AccessToken.Split('.').Should().HaveCount(3);
        token.ExpiresAtUtc.Should().BeAfter(DateTimeOffset.UtcNow);

        var handler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token.AccessToken);
        jwt.GetClaim("sub").Value.Should().Be(owner.Id.ToString());
        jwt.GetClaim("tenant_id").Value.Should().Be(account.Id.ToString());
        jwt.GetClaim("role").Value.Should().Be("Owner");
    }
}
