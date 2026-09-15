using OzapTalk.SharedKernel.Domain;

namespace OzapTalk.Accounts.Domain.Accounts;

public sealed record AccountRegisteredEvent(Guid AccountId, string CompanyName, Guid OwnerUserId, string OwnerEmail)
    : IDomainEvent
{
    public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;
}
