using System.Linq.Expressions;
using Hangfire;
using SopaTalk.SharedKernel.Jobs;

namespace SopaTalk.Api.Jobs;

/// <summary>
/// Liga <see cref="IRecurringJobRegistry"/> ao Hangfire. É o único ponto do host que sabe
/// qual agendador está em uso.
///
/// O agendamento é gravado no Postgres, então basta um processo registrar: o host da API
/// registra, e qualquer servidor Hangfire (inclusive o SopaTalk.Workers) executa o que
/// estiver na fila.
/// </summary>
internal sealed class HangfireRecurringJobRegistry(IRecurringJobManager manager) : IRecurringJobRegistry
{
    public void Schedule<TJob>(string jobId, string cron, Expression<Func<TJob, Task>> method)
        where TJob : notnull
        => manager.AddOrUpdate(jobId, method, cron, new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
}
