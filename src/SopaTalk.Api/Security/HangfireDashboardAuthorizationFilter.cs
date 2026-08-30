using Hangfire.Dashboard;

namespace SopaTalk.Api.Security;

/// <summary>
/// Gate for the Hangfire dashboard. The dashboard is a browser UI and does not carry
/// our bearer token, so for now it is only reachable in Development. Production access
/// will go through a dedicated admin cookie/session — tracked in the R0 checklist.
/// </summary>
public sealed class HangfireDashboardAuthorizationFilter(IWebHostEnvironment environment) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => environment.IsDevelopment();
}
