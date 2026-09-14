namespace SopaTalk.SharedKernel.Notifications;

/// <summary>
/// Um e-mail pronto para envio. O remetente não vem aqui: ele é configuração da
/// instalação (<see cref="EmailOptions"/>), não decisão de quem escreve a mensagem.
/// </summary>
/// <param name="To">Destinatário único. Para vários, envie uma mensagem por pessoa —
/// mantém a personalização e evita expor a lista.</param>
/// <param name="TextBody">Alternativa em texto puro. Opcional, mas melhora a entrega:
/// provedores penalizam mensagens só-HTML.</param>
public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string? TextBody = null);
