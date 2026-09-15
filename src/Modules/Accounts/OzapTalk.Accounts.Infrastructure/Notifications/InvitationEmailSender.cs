using OzapTalk.Accounts.Application.Abstractions;
using OzapTalk.Accounts.Domain.Users;
using OzapTalk.SharedKernel.Notifications;

namespace OzapTalk.Accounts.Infrastructure.Notifications;

/// <summary>
/// Monta o e-mail de convite e entrega pelo remetente configurado no host. O módulo não
/// sabe se por trás está um arquivo em disco ou um provedor real — só redige a mensagem.
/// </summary>
internal sealed class InvitationEmailSender(IEmailSender emails) : IInvitationEmailSender
{
    public Task SendAsync(InvitationEmail email, CancellationToken ct) =>
        emails.SendAsync(new EmailMessage(
            email.ToEmail,
            $"{email.InviterName} convidou você para {email.CompanyName}",
            $"""
            <p>Olá.</p>
            <p>{email.InviterName} convidou você para participar de <strong>{email.CompanyName}</strong>
            no ozap-talk como <strong>{Describe(email.Role)}</strong>.</p>
            <p><a href="{email.AcceptUrl}">Aceitar o convite</a></p>
            """,
            $"""
            Olá.

            {email.InviterName} convidou você para participar de {email.CompanyName} no
            ozap-talk como {Describe(email.Role)}.

            Aceite o convite em: {email.AcceptUrl}
            """), ct);

    private static string Describe(MembershipRole role) => role switch
    {
        MembershipRole.Owner => "Proprietário",
        MembershipRole.Admin => "Administrador",
        MembershipRole.Operator => "Operador",
        _ => "Membro",
    };
}
