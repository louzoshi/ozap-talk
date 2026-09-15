using Microsoft.Extensions.Options;
using OzapTalk.Accounts.Application.Abstractions;

namespace OzapTalk.Accounts.Infrastructure.Notifications;

internal sealed class InviteLinkBuilder(IOptions<AccountsOptions> options) : IInviteLinkBuilder
{
    private readonly string _baseUrl = options.Value.AppBaseUrl.TrimEnd('/');

    public string BuildAcceptUrl(string rawToken) => $"{_baseUrl}/convite/{rawToken}";
}
