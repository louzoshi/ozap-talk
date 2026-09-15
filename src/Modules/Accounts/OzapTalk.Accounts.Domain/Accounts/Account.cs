using OzapTalk.Accounts.Domain.Users;
using OzapTalk.SharedKernel.Domain;
using OzapTalk.SharedKernel.Results;

namespace OzapTalk.Accounts.Domain.Accounts;

public enum PlanTier
{
    Trial = 0,
    Essential = 1,
    Growth = 2,
    Scale = 3,
}

/// <summary>
/// A customer company. Its <see cref="Entity.Id"/> IS the tenant id that every other
/// module scopes its data by. Registering an account also creates its first user (the owner).
/// </summary>
public sealed class Account : AggregateRoot
{
    private Account(Guid id, string name, string slug) : base(id)
    {
        Name = name;
        Slug = slug;
        Plan = PlanTier.Trial;
        IsActive = true;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    // EF Core
    private Account() { }

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public PlanTier Plan { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Result<(Account Account, User Owner)> Register(
        string companyName, string slug, string ownerEmail, string ownerDisplayName, string ownerPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            return AccountErrors.CompanyNameRequired;
        if (string.IsNullOrWhiteSpace(slug))
            return AccountErrors.SlugRequired;

        var account = new Account(Guid.CreateVersion7(), companyName.Trim(), slug.Trim().ToLowerInvariant());
        var owner = User.Create(account.Id, ownerEmail, ownerDisplayName, ownerPasswordHash, MembershipRole.Owner);
        account.Raise(new AccountRegisteredEvent(account.Id, account.Name, owner.Id, owner.Email));
        return (account, owner);
    }

    public void Deactivate() => IsActive = false;
}
