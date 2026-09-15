using System.Linq.Expressions;

namespace OzapTalk.SharedKernel.Jobs;

/// <summary>
/// Onde um módulo declara seus jobs recorrentes. O host implementa isto sobre o agendador
/// real, de modo que o módulo não referencia Hangfire e o host não conhece o job — só
/// entrega o registro e chama o módulo.
/// </summary>
public interface IRecurringJobRegistry
{
    /// <param name="jobId">Identificador estável, prefixado pelo módulo
    /// (ex.: <c>accounts:weekly-digest</c>). Reagendar com o mesmo id atualiza o cron
    /// em vez de criar um job duplicado.</param>
    /// <param name="cron">Expressão cron interpretada em UTC.</param>
    void Schedule<TJob>(string jobId, string cron, Expression<Func<TJob, Task>> method) where TJob : notnull;
}
