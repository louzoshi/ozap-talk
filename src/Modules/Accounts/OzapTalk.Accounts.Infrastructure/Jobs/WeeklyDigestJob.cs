using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OzapTalk.Accounts.Domain.Users;
using OzapTalk.Accounts.Infrastructure.Persistence;
using OzapTalk.SharedKernel.MultiTenancy;
using OzapTalk.SharedKernel.Notifications;

namespace OzapTalk.Accounts.Infrastructure.Jobs;

/// <summary>
/// Resumo semanal enviado a cada empresa. Hoje o corpo é um esqueleto — o conteúdo real
/// (volume de conversas, tempo de resposta, atendente) entra quando os relatórios do R1
/// existirem.
///
/// O padrão que importa aqui é o fan-out por tenant: o job roda uma vez, sem tenant algum
/// em escopo, e abre um escopo por empresa definindo o <see cref="ISettableTenantContext"/>
/// antes de tocar no banco. Sem isso o filtro global do EF Core devolve vazio.
/// </summary>
public sealed class WeeklyDigestJob(
    IServiceScopeFactory scopeFactory,
    ILogger<WeeklyDigestJob> logger)
{
    public async Task RunAsync(CancellationToken ct)
    {
        Guid[] accountIds;

        // Account é o próprio tenant e não tem filtro global, então a listagem roda fora
        // de qualquer escopo de tenant.
        await using (var scope = scopeFactory.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
            accountIds = await db.Accounts
                .Where(a => a.IsActive)
                .Select(a => a.Id)
                .ToArrayAsync(ct);
        }

        logger.LogInformation("Resumo semanal: {Count} empresa(s) ativa(s)", accountIds.Length);

        var failures = 0;
        foreach (var accountId in accountIds)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                await SendForAccountAsync(accountId, ct);
            }
            catch (Exception ex)
            {
                // Uma empresa com problema não pode derrubar o resumo das outras.
                failures++;
                logger.LogError(ex, "Falha ao enviar o resumo semanal da conta {AccountId}", accountId);
            }
        }

        if (failures > 0)
            logger.LogWarning("Resumo semanal concluído com {Failures} falha(s)", failures);
    }

    private async Task SendForAccountAsync(Guid accountId, CancellationToken ct)
    {
        // Um escopo por empresa: o contexto de tenant e o DbContext são scoped, e reaproveitar
        // o escopo entre tenants vazaria dados de uma empresa para outra.
        await using var scope = scopeFactory.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<ISettableTenantContext>().SetTenant(accountId);

        var db = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
        var emails = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, ct);
        if (account is null)
            return;

        // Users é filtrado pelo tenant definido acima.
        var recipients = await db.Users
            .Where(u => u.IsActive && (u.Role == MembershipRole.Owner || u.Role == MembershipRole.Admin))
            .Select(u => new { u.Email, u.DisplayName })
            .ToArrayAsync(ct);

        foreach (var recipient in recipients)
        {
            await emails.SendAsync(new EmailMessage(
                recipient.Email,
                $"Resumo semanal — {account.Name}",
                HtmlBody(recipient.DisplayName, account.Name),
                TextBody(recipient.DisplayName, account.Name)), ct);
        }

        logger.LogInformation(
            "Resumo semanal de {Company}: {Count} destinatário(s)", account.Name, recipients.Length);
    }

    private static string HtmlBody(string displayName, string companyName) =>
        $"""
        <p>Olá, {displayName}.</p>
        <p>Este é o resumo semanal de <strong>{companyName}</strong>.</p>
        <p>Os números do período entram aqui quando os relatórios estiverem prontos.</p>
        """;

    private static string TextBody(string displayName, string companyName) =>
        $"""
        Olá, {displayName}.

        Este é o resumo semanal de {companyName}.
        Os números do período entram aqui quando os relatórios estiverem prontos.
        """;
}
