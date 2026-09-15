using OzapTalk.SharedKernel.MultiTenancy;

namespace OzapTalk.Api.Security;

/// <summary>
/// Copies the <c>tenant_id</c> claim of the authenticated principal into the scoped
/// <see cref="ISettableTenantContext"/>. Runs after authentication, before authorization.
/// Anonymous requests leave the tenant context empty.
/// </summary>
public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ISettableTenantContext tenantContext)
    {
        var tenantClaim = context.User.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(tenantClaim, out var tenantId))
            tenantContext.SetTenant(tenantId);

        await next(context);
    }
}
