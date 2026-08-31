using SopaTalk.Accounts.Domain.Users;

namespace SopaTalk.Accounts.Application.Abstractions;

public sealed record InvitationEmail(
    string ToEmail,
    string CompanyName,
    string InviterName,
    MembershipRole Role,
    string AcceptUrl);

/// <summary>
/// Entrega o convite à pessoa. Em desenvolvimento a implementação apenas registra o link
/// no log; um provedor real (SMTP/SES/Resend) entra quando for para produção.
/// </summary>
public interface IInvitationEmailSender
{
    Task SendAsync(InvitationEmail email, CancellationToken ct);
}
