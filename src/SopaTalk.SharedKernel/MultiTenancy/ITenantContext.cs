namespace SopaTalk.SharedKernel.MultiTenancy;

/// <summary>
/// Ambient tenant for the current request or job. Resolved from the authenticated principal
/// (claim <c>tenant_id</c>) in the API, and from job arguments in the workers.
/// Every module DbContext applies a global query filter on this value, and Postgres
/// Row-Level Security is the second line of defence.
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; }
    bool HasTenant { get; }
}
