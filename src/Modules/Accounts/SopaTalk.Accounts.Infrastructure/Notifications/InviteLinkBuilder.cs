using Microsoft.Extensions.Options;
using SopaTalk.Accounts.Application.Abstractions;

namespace SopaTalk.Accounts.Infrastructure.Notifications;

internal sealed class InviteLinkBuilder(IOptions<AccountsOptions> options) : IInviteLinkBuilder
{
    private readonly string _baseUrl = options.Value.AppBaseUrl.TrimEnd('/');

    public string BuildAcceptUrl(string rawToken) => $"{_baseUrl}/convite/{rawToken}";
}
