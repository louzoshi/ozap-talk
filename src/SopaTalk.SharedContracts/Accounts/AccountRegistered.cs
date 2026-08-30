using SopaTalk.SharedKernel.Messaging;

namespace SopaTalk.SharedContracts.Accounts;

/// <summary>A new company signed up. Other modules seed their per-tenant defaults from this.</summary>
public sealed record AccountRegistered(Guid AccountId, string CompanyName, Guid OwnerUserId, string OwnerEmail)
    : IntegrationEvent;
