namespace OzapTalk.SharedKernel.MultiTenancy;

/// <summary>
/// Marks an entity as belonging to exactly one tenant. Implementers get:
/// a global query filter (reads only ever see the current tenant) and an interceptor
/// that stamps <see cref="TenantId"/> on insert. Every persistent entity that holds
/// customer data must implement this.
/// </summary>
public interface ITenantOwned
{
    Guid TenantId { get; }
}
