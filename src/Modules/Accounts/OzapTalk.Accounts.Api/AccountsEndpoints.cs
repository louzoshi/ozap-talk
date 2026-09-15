using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OzapTalk.Accounts.Application.Authentication;
using OzapTalk.SharedKernel.Http;

namespace OzapTalk.Accounts.Api;

internal static class AccountsEndpoints
{
    public static void MapAccountsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/accounts").WithTags("Accounts");

        group.MapPost("/register", async (
            RegisterAccountCommand command, RegisterAccountHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);
            return result.Match(auth => Results.Ok(auth));
        })
        .AllowAnonymous()
        .WithSummary("Register a new company and its owner user");

        group.MapPost("/sessions", async (
            AuthenticateCommand command, AuthenticateHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);
            return result.Match(auth => Results.Ok(auth));
        })
        .AllowAnonymous()
        .WithSummary("Sign in with e-mail and password");

        group.MapGet("/me", async (ClaimsPrincipal principal, GetCurrentUserHandler handler, CancellationToken ct) =>
        {
            var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue("sub");
            if (!Guid.TryParse(sub, out var userId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(userId, ct);
            return result.Match(user => Results.Ok(user));
        })
        .RequireAuthorization()
        .WithSummary("The signed-in user and their company");
    }
}
