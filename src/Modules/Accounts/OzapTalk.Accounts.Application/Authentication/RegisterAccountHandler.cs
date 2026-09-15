using FluentValidation;
using OzapTalk.Accounts.Application.Abstractions;
using OzapTalk.Accounts.Domain.Accounts;
using OzapTalk.Accounts.Domain.Users;
using OzapTalk.SharedContracts.Accounts;
using OzapTalk.SharedKernel.Messaging;
using OzapTalk.SharedKernel.Results;

namespace OzapTalk.Accounts.Application.Authentication;

public sealed record RegisterAccountCommand(
    string CompanyName,
    string Slug,
    string OwnerName,
    string Email,
    string Password);

public sealed class RegisterAccountCommandValidator : AbstractValidator<RegisterAccountCommand>
{
    public RegisterAccountCommandValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9][a-z0-9-]{1,38}[a-z0-9]$")
            .WithMessage("O identificador usa apenas letras minúsculas, números e hífen.");
        RuleFor(x => x.OwnerName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(256);
    }
}

public sealed class RegisterAccountHandler(
    IAccountRepository accounts,
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer tokenIssuer,
    IAccountsUnitOfWork unitOfWork,
    IEventBus eventBus,
    IValidator<RegisterAccountCommand> validator)
{
    public async Task<Result<AuthenticationResult>> HandleAsync(RegisterAccountCommand command, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return Error.Validation("account.invalid", validation.Errors[0].ErrorMessage);

        var slug = command.Slug.Trim().ToLowerInvariant();
        if (await accounts.SlugExistsAsync(slug, ct))
            return AccountErrors.SlugTaken;

        var email = User.Normalize(command.Email);
        if (await users.EmailExistsAsync(email, ct))
            return AccountErrors.EmailTaken;

        var passwordHash = passwordHasher.Hash(command.Password);

        var registration = Account.Register(command.CompanyName, slug, email, command.OwnerName, passwordHash);
        if (registration.IsFailure)
            return registration.Error;

        var (account, owner) = registration.Value;
        accounts.Add(account);
        users.Add(owner);
        await unitOfWork.SaveChangesAsync(ct);

        await eventBus.PublishAsync(
            new AccountRegistered(account.Id, account.Name, owner.Id, owner.Email), ct);

        var token = tokenIssuer.Issue(owner, account);
        return new AuthenticationResult(
            token.AccessToken,
            token.ExpiresAtUtc,
            new CurrentUser(owner.Id, account.Id, owner.Email, owner.DisplayName, owner.Role.ToString(), account.Name));
    }
}
