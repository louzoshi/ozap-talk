using Microsoft.EntityFrameworkCore;
using OzapTalk.SharedKernel.MultiTenancy;

namespace OzapTalk.SharedKernel.Persistence;

/// <summary>
/// Base for every module DbContext. Provides <see cref="CurrentTenantId"/> for global
/// query filters and wires <see cref="TenantSaveChangesInterceptor"/> so inserts are
/// stamped and cross-tenant writes are blocked.
///
/// Each derived context still declares its own filters explicitly, one line per
/// tenant-owned entity:
/// <code>
/// modelBuilder.Entity&lt;User&gt;().HasQueryFilter(u =&gt; u.TenantId == CurrentTenantId);
/// </code>
/// The filter references <see cref="CurrentTenantId"/> (a member of the context), which
/// EF Core re-binds to the executing instance on every query — safe with the cached model.
/// </summary>
public abstract class TenantDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    protected TenantDbContext(DbContextOptions options, ITenantContext tenantContext) : base(options)
        => _tenantContext = tenantContext;

    /// <summary>Current tenant, or <see cref="Guid.Empty"/> when no tenant is in scope
    /// (registration, design-time). A filter comparing against Empty simply returns nothing.</summary>
    protected Guid CurrentTenantId => _tenantContext.HasTenant ? _tenantContext.TenantId : Guid.Empty;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new TenantSaveChangesInterceptor(_tenantContext));
        base.OnConfiguring(optionsBuilder);
    }
}
