using FluentValidation;
using SopaTalk.Accounts.Application.Abstractions;
using SopaTalk.Accounts.Application.Authentication;
using SopaTalk.Accounts.Domain.Accounts;
using SopaTalk.Accounts.Domain.Invitations;
using SopaTalk.Accounts.Domain.Users;
using SopaTalk.SharedKernel.Results;

namespace SopaTalk.Accounts.Application.Team;

public sealed record AcceptInvitationCommand(string Token, string DisplayName, string Password);

public sealed class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(256);
    }
}

public sealed class AcceptInvitationHandler(
    IInvitationRepository invitations,
    IUserRepository users,
    IAccountRepository accounts,
    IInvitationTokens tokens,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer tokenIssuer,
    IAccountsUnitOfWork unitOfWork,
    IValidator<AcceptInvitationCommand> validator)
{
    public async Task<Result<AuthenticationResult>> HandleAsync(AcceptInvitationCommand command, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return Error.Validation("invitation.invalid", validation.Errors[0].ErrorMessage);

        var invitation = await invitations.FindByTokenHashAsync(tokens.Hash(command.Token), ct);
        if (invitation is null)
            return InvitationErrors.NotFound;

        var now = DateTimeOffset.UtcNow;
        if (!invitation.IsUsable(now))
            return invitation.Status == InvitationStatus.Pending
                ? InvitationErrors.Expired
                : InvitationErrors.NotPending;

        if (await users.EmailExistsAsync(invitation.Email, ct))
            return InvitationErrors.EmailAlreadyInUse;

        var account = await accounts.GetAsync(invitation.TenantId, ct);
        if (account is null || !account.IsActive)
            return AccountErrors.NotFound;

        var user = User.Create(
            invitation.TenantId, invitation.Email, command.DisplayName,
            passwordHasher.Hash(command.Password), invitation.Role);

        var accepted = invitation.Accept(now);
        if (accepted.IsFailure)
            return accepted.Error;

        user.MarkSignedIn();
        users.Add(user);
        await unitOfWork.SaveChangesAsync(ct);

        var token = tokenIssuer.Issue(user, account);
        return new AuthenticationResult(
            token.AccessToken,
            token.ExpiresAtUtc,
            new CurrentUser(user.Id, account.Id, user.Email, user.DisplayName, user.Role.ToString(), account.Name));
    }
}
