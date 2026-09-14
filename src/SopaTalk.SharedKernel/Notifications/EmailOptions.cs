using System.ComponentModel.DataAnnotations;

namespace SopaTalk.SharedKernel.Notifications;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>Nome exibido como remetente.</summary>
    [Required]
    public string FromName { get; init; } = "sopa-talk";

    /// <summary>
    /// Endereço remetente. Com domínio próprio, precisa estar verificado no provedor
    /// (SPF/DKIM) — senão a mensagem cai em spam ou é recusada. Para testar sem domínio,
    /// o Resend aceita <c>onboarding@resend.dev</c>, que só entrega no e-mail da própria conta.
    /// </summary>
    [Required]
    public string FromAddress { get; init; } = "nao-responda@localhost";

    /// <summary>Pasta onde o remetente de desenvolvimento grava os .eml.</summary>
    public string DropFolder { get; init; } = "tmp/emails";

    /// <summary>
    /// Chave da API do Resend. Vazia (o padrão) mantém o remetente de desenvolvimento,
    /// que grava em disco em vez de enviar. Nunca no git — use user-secrets ou variável
    /// de ambiente (<c>Email__ApiKey</c>).
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;
}
