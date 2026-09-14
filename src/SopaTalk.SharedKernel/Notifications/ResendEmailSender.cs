using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SopaTalk.SharedKernel.Notifications;

/// <summary>
/// Envio real pela API do Resend. Uma falha vira exceção de propósito: o job que chamou
/// está sob o Hangfire, e é ele quem decide repetir.
/// </summary>
public sealed class ResendEmailSender(
    HttpClient http,
    IOptions<EmailOptions> options,
    ILogger<ResendEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        var settings = options.Value;

        var response = await http.PostAsJsonAsync("emails", new
        {
            from = $"{settings.FromName} <{settings.FromAddress}>",
            to = new[] { message.To },
            subject = message.Subject,
            html = message.HtmlBody,
            text = message.TextBody,
        }, ct);

        if (!response.IsSuccessStatusCode)
        {
            // O corpo do erro do Resend diz o motivo (domínio não verificado, destinatário
            // fora da conta no modo de teste, chave inválida) — sem ele o diagnóstico é cego.
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Resend recusou o envio para {message.To} ({(int)response.StatusCode}): {body}");
        }

        logger.LogInformation("E-mail enviado para {To} (assunto: {Subject})", message.To, message.Subject);
    }
}
