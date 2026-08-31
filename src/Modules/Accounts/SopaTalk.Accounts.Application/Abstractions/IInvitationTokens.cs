namespace SopaTalk.Accounts.Application.Abstractions;

public sealed record InvitationToken(string Raw, string Hash);

/// <summary>
/// Gera o token do convite. O valor cru (<see cref="InvitationToken.Raw"/>) vai no link
/// enviado à pessoa; só o <see cref="InvitationToken.Hash"/> é persistido.
/// </summary>
public interface IInvitationTokens
{
    InvitationToken Create();

    string Hash(string rawToken);
}
