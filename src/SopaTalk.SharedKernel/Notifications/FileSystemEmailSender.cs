using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SopaTalk.SharedKernel.Notifications;

/// <summary>
/// Remetente de desenvolvimento: grava cada mensagem como um arquivo <c>.eml</c> — que
/// abre em qualquer cliente de e-mail — e registra uma linha no log. Não envia nada para
/// fora, então dá para exercitar todo o fluxo sem provedor, sem domínio e sem risco de
/// disparar e-mail para cliente de verdade a partir de uma máquina de desenvolvimento.
/// </summary>
public sealed class FileSystemEmailSender(
    IOptions<EmailOptions> options,
    ILogger<FileSystemEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        var settings = options.Value;
        var folder = Path.GetFullPath(settings.DropFolder);
        Directory.CreateDirectory(folder);

        var path = Path.Combine(folder, $"{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}-{Sanitize(message.To)}.eml");
        await File.WriteAllTextAsync(path, Compose(settings, message), Encoding.UTF8, ct);

        logger.LogInformation(
            "E-mail para {To} gravado em {Path} (assunto: {Subject})", message.To, path, message.Subject);
    }

    private static string Compose(EmailOptions settings, EmailMessage message)
    {
        var builder = new StringBuilder();
        builder.Append("From: ").Append(settings.FromName).Append(" <").Append(settings.FromAddress).AppendLine(">");
        builder.Append("To: ").AppendLine(message.To);
        builder.Append("Subject: ").AppendLine(message.Subject);
        builder.Append("Date: ").AppendLine(DateTimeOffset.UtcNow.ToString("r", CultureInfo.InvariantCulture));
        builder.AppendLine("MIME-Version: 1.0");

        if (message.TextBody is null)
        {
            builder.AppendLine("Content-Type: text/html; charset=utf-8").AppendLine();
            builder.AppendLine(message.HtmlBody);
            return builder.ToString();
        }

        const string boundary = "sopa-talk-boundary";
        builder.Append("Content-Type: multipart/alternative; boundary=\"").Append(boundary).AppendLine("\"").AppendLine();
        builder.Append("--").AppendLine(boundary);
        builder.AppendLine("Content-Type: text/plain; charset=utf-8").AppendLine();
        builder.AppendLine(message.TextBody);
        builder.Append("--").AppendLine(boundary);
        builder.AppendLine("Content-Type: text/html; charset=utf-8").AppendLine();
        builder.AppendLine(message.HtmlBody);
        builder.Append("--").Append(boundary).AppendLine("--");
        return builder.ToString();
    }

    /// <summary>Deixa o endereço utilizável como nome de arquivo.</summary>
    private static string Sanitize(string address)
    {
        var chars = address.Select(c => Path.GetInvalidFileNameChars().Contains(c) || c == '@' ? '_' : c);
        return new string(chars.ToArray());
    }
}
