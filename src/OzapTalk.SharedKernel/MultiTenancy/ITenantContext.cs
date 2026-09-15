namespace OzapTalk.SharedKernel.MultiTenancy;

/// <summary>
/// Ambient tenant for the current request or job. Resolved from the authenticated
/// principal (claim <c>tenant_id</c>) in the API, and set explicitly from job
/// arguments in the workers.
/// Every module DbContext applies a global query filter on this value, and the
/// tenant interceptor stamps it on new rows. Postgres Row-Level Security is the
/// planned second line of defence (see docs/adr/0004).
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; }
    bool HasTenant { get; }
}

/// <summary>Lets infrastructure set the tenant for a scope (auth middleware, job entry point).</summary>
public interface ISettableTenantContext : ITenantContext
{
    void SetTenant(Guid tenantId);
}

/// <summary>Scoped. One instance per request / per job execution.</summary>
public sealed class TenantContext : ISettableTenantContext
{
    private Guid? _tenantId;

    public Guid TenantId => _tenantId
        ?? throw new InvalidOperationException(
            "No tenant in scope. The request is not authenticated, or a job did not set its tenant.");

    public bool HasTenant => _tenantId.HasValue;

    public void SetTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant id cannot be empty.", nameof(tenantId));

        _tenantId = tenantId;
    }
}
