namespace SopaTalk.SharedKernel.Notifications;

/// <summary>
/// Entrega de e-mail transacional. A implementação é escolhida no host: em
/// desenvolvimento grava o arquivo em disco, em produção entra um provedor real
/// (Resend/SES/SMTP). Quem chama nunca sabe qual está ativo.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct);
}
