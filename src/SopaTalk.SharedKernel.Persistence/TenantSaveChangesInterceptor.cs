using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SopaTalk.SharedKernel.MultiTenancy;

namespace SopaTalk.SharedKernel.Persistence;

/// <summary>
/// Enforces tenant ownership on write:
/// <list type="bullet">
///   <item>Insert with an ambient tenant → stamps <see cref="ITenantOwned.TenantId"/>.</item>
///   <item>Insert with no ambient tenant → allowed only if the entity already carries a
///         non-empty tenant id (account registration bootstraps its own owner user).</item>
///   <item>Update/delete whose tenant id differs from the ambient tenant → blocked.</item>
/// </list>
/// </summary>
public sealed class TenantSaveChangesInterceptor(ITenantContext tenantContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext? context)
    {
        if (context is null)
            return;

        Guid? ambient = tenantContext.HasTenant ? tenantContext.TenantId : null;

        foreach (var entry in context.ChangeTracker.Entries<ITenantOwned>())
        {
            var property = entry.Property(nameof(ITenantOwned.TenantId));
            var current = (Guid)(property.CurrentValue ?? Guid.Empty);

            switch (entry.State)
            {
                case EntityState.Added when ambient is { } a:
                    if (current == Guid.Empty)
                        property.CurrentValue = a;
                    else if (current != a)
                        throw CrossTenant(entry);
                    break;

                case EntityState.Added:
                    if (current == Guid.Empty)
                        throw new InvalidOperationException(
                            $"No tenant in scope and {entry.Entity.GetType().Name} has no tenant id set.");
                    break;

                case (EntityState.Modified or EntityState.Deleted) when ambient is { } a && current != a:
                    throw CrossTenant(entry);
            }
        }
    }

    private static InvalidOperationException CrossTenant(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry) =>
        new($"Cross-tenant write blocked on {entry.Entity.GetType().Name}.");
}
