using Microsoft.Extensions.Logging;
using SopaTalk.Accounts.Application.Abstractions;

namespace SopaTalk.Accounts.Infrastructure.Notifications;

/// <summary>
/// Implementação de desenvolvimento: registra o link de aceite no log em vez de enviar
/// e-mail. Trocar por um provedor real (SMTP/SES/Resend) antes de produção.
/// </summary>
internal sealed class LoggingInvitationEmailSender(ILogger<LoggingInvitationEmailSender> logger)
    : IInvitationEmailSender
{
    public Task SendAsync(InvitationEmail email, CancellationToken ct)
    {
        logger.LogInformation(
            "Convite para {Email} como {Role} em {Company} (por {Inviter}): {AcceptUrl}",
            email.ToEmail, email.Role, email.CompanyName, email.InviterName, email.AcceptUrl);
        return Task.CompletedTask;
    }
}
