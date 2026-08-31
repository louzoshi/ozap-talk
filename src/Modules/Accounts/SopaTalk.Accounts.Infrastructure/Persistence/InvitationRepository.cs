using Microsoft.EntityFrameworkCore;
using SopaTalk.Accounts.Application.Abstractions;
using SopaTalk.Accounts.Domain.Invitations;

namespace SopaTalk.Accounts.Infrastructure.Persistence;

internal sealed class InvitationRepository(AccountsDbContext db) : IInvitationRepository
{
    // Accepting an invitation is anonymous — no tenant in scope — so this lookup
    // deliberately bypasses the tenant query filter.
    public Task<Invitation?> FindByTokenHashAsync(string tokenHash, CancellationToken ct) =>
        db.Invitations.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.TokenHash == tokenHash, ct);

    public Task<bool> HasPendingForEmailAsync(string normalizedEmail, CancellationToken ct) =>
        db.Invitations.AnyAsync(
            i => i.Email == normalizedEmail && i.Status == InvitationStatus.Pending, ct);

    public async Task<IReadOnlyList<Invitation>> ListPendingAsync(CancellationToken ct) =>
        await db.Invitations
            .AsNoTracking()
            .Where(i => i.Status == InvitationStatus.Pending)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync(ct);

    public Task<Invitation?> GetAsync(Guid invitationId, CancellationToken ct) =>
        db.Invitations.FirstOrDefaultAsync(i => i.Id == invitationId, ct);

    public void Add(Invitation invitation) => db.Invitations.Add(invitation);
}
