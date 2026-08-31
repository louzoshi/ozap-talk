namespace SopaTalk.Accounts.Infrastructure;

/// <summary>Bound from the <c>Accounts</c> configuration section.</summary>
public sealed class AccountsOptions
{
    public const string SectionName = "Accounts";

    /// <summary>Base pública da SPA — usada para montar o link de aceite do convite.</summary>
    public string AppBaseUrl { get; init; } = "http://localhost:5173";
}
