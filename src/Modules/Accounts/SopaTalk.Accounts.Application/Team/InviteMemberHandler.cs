using FluentValidation;
using SopaTalk.Accounts.Application.Abstractions;
using SopaTalk.Accounts.Domain.Accounts;
using SopaTalk.Accounts.Domain.Invitations;
using SopaTalk.Accounts.Domain.Users;
using SopaTalk.SharedKernel.Results;

namespace SopaTalk.Accounts.Application.Team;

public sealed record InviteMemberCommand(Guid ActorUserId, string Email, MembershipRole Role);

public sealed class InviteMemberCommandValidator : AbstractValidator<InviteMemberCommand>
{
    public InviteMemberCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Role).IsInEnum();
    }
}

public sealed class InviteMemberHandler(
    IUserRepository users,
    IAccountRepository accounts,
    IInvitationRepository invitations,
    IInvitationTokens tokens,
    IInvitationEmailSender emailSender,
    IInviteLinkBuilder linkBuilder,
    IAccountsUnitOfWork unitOfWork,
    IValidator<InviteMemberCommand> validator)
{
    /// <summary>Validade do convite. Configurável vira opção quando houver demanda.</summary>
    public static readonly TimeSpan Ttl = TimeSpan.FromHours(72);

    public async Task<Result<InvitationCreated>> HandleAsync(InviteMemberCommand command, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return Error.Validation("invitation.invalid", validation.Errors[0].ErrorMessage);

        var actor = await users.GetAsync(command.ActorUserId, ct);
        if (actor is null)
            return TeamErrors.ActorNotFound;

        if (!MembershipRules.CanManage(actor.Role, command.Role))
            return InvitationErrors.Forbidden;

        var email = User.Normalize(command.Email);
        if (await users.EmailExistsAsync(email, ct))
            return InvitationErrors.EmailAlreadyInUse;
        if (await invitations.HasPendingForEmailAsync(email, ct))
            return InvitationErrors.PendingInvitationExists;

        var account = await accounts.GetAsync(actor.TenantId, ct);
        if (account is null)
            return AccountErrors.NotFound;

        var token = tokens.Create();
        var creation = Invitation.Create(actor.TenantId, email, command.Role, actor.Id, token.Hash, Ttl);
        if (creation.IsFailure)
            return creation.Error;

        var invitation = creation.Value;
        invitations.Add(invitation);
        await unitOfWork.SaveChangesAsync(ct);

        var acceptUrl = linkBuilder.BuildAcceptUrl(token.Raw);
        await emailSender.SendAsync(
            new InvitationEmail(email, account.Name, actor.DisplayName, command.Role, acceptUrl), ct);

        return new InvitationCreated(ToSummary(invitation), acceptUrl);
    }

    internal static InvitationSummary ToSummary(Invitation i) =>
        new(i.Id, i.Email, i.Role, i.CreatedAtUtc, i.ExpiresAtUtc);
}
