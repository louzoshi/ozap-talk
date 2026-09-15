using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OzapTalk.Api.Realtime;

/// <summary>
/// Push channel for the inbox UI. Each connection joins a group for its tenant, so a
/// conversation update only reaches that tenant's agents.
/// </summary>
[Authorize]
public sealed class InboxHub : Hub
{
    public static string TenantGroup(Guid tenantId) => $"tenant:{tenantId}";

    public override async Task OnConnectedAsync()
    {
        var tenantId = Context.User?.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(tenantId, out var id))
            await Groups.AddToGroupAsync(Context.ConnectionId, TenantGroup(id));

        await base.OnConnectedAsync();
    }
}
