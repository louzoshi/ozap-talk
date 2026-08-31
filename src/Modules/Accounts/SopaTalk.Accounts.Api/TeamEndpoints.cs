using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SopaTalk.Accounts.Application.Team;
using SopaTalk.Accounts.Domain.Users;
using SopaTalk.SharedKernel.Http;

namespace SopaTalk.Accounts.Api;

internal static class TeamEndpoints
{
    public static void MapTeamEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/accounts").WithTags("Team");

        // --- Convites (público: abrir e aceitar) ---
        group.MapGet("/invite/{token}", async (string token, GetInvitationHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(token, ct);
            return result.Match(preview => Results.Ok(preview));
        }).AllowAnonymous().WithSummary("Prévia de um convite pelo token");

        group.MapPost("/invite/{token}", async (
            string token, AcceptInviteBody body, AcceptInvitationHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(
                new AcceptInvitationCommand(token, body.DisplayName, body.Password), ct);
            return result.Match(auth => Results.Ok(auth));
        }).AllowAnonymous().WithSummary("Aceitar um convite e criar o usuário");

        // --- Convites (gestão) ---
        group.MapPost("/invitations", async (
            InviteMemberBody body, ClaimsPrincipal principal, InviteMemberHandler handler, CancellationToken ct) =>
        {
            if (!TryGetUserId(principal, out var actorId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(new InviteMemberCommand(actorId, body.Email, body.Role), ct);
            return result.Match(created => Results.Created($"/api/accounts/invitations/{created.Invitation.Id}", created));
        }).RequireAuthorization("admin").WithSummary("Convidar alguém para a conta");

        group.MapGet("/invitations", async (ListInvitationsHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.Match(list => Results.Ok(list));
        }).RequireAuthorization("admin").WithSummary("Listar convites pendentes");

        group.MapDelete("/invitations/{id:guid}", async (
            Guid id, ClaimsPrincipal principal, RevokeInvitationHandler handler, CancellationToken ct) =>
        {
            if (!TryGetUserId(principal, out var actorId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(new RevokeInvitationCommand(actorId, id), ct);
            return result.Match(Results.NoContent);
        }).RequireAuthorization("admin").WithSummary("Cancelar um convite");

        // --- Membros ---
        group.MapGet("/members", async (ListMembersHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.Match(list => Results.Ok(list));
        }).RequireAuthorization("admin").WithSummary("Listar membros da conta");

        group.MapPatch("/members/{id:guid}/role", async (
            Guid id, ChangeRoleBody body, ClaimsPrincipal principal,
            ChangeMemberRoleHandler handler, CancellationToken ct) =>
        {
            if (!TryGetUserId(principal, out var actorId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(new ChangeMemberRoleCommand(actorId, id, body.Role), ct);
            return result.Match(Results.NoContent);
        }).RequireAuthorization("admin").WithSummary("Trocar o papel de um membro");

        group.MapDelete("/members/{id:guid}", async (
            Guid id, ClaimsPrincipal principal, DeactivateMemberHandler handler, CancellationToken ct) =>
        {
            if (!TryGetUserId(principal, out var actorId))
                return Results.Unauthorized();

            var result = await handler.HandleAsync(new DeactivateMemberCommand(actorId, id), ct);
            return result.Match(Results.NoContent);
        }).RequireAuthorization("admin").WithSummary("Desativar um membro");
    }

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(
            principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub"), out userId);

    private sealed record InviteMemberBody(string Email, MembershipRole Role);
    private sealed record AcceptInviteBody(string DisplayName, string Password);
    private sealed record ChangeRoleBody(MembershipRole Role);
}
