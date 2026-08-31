namespace SopaTalk.Accounts.Application.Abstractions;

/// <summary>Monta a URL pública de aceite do convite a partir do token cru.</summary>
public interface IInviteLinkBuilder
{
    string BuildAcceptUrl(string rawToken);
}
