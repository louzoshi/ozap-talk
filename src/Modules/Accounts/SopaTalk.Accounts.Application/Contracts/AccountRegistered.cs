using SopaTalk.SharedKernel.Messaging;

namespace SopaTalk.Accounts.Application.Contracts;

/// <summary>
/// Public: a new company signed up. Other modules seed their per-tenant defaults
/// from this (Inbox creates a default team, etc.). Versioned — treat as API.
/// </summary>
public sealed record AccountRegistered(Guid AccountId, string CompanyName, Guid OwnerUserId, string OwnerEmail)
    : IntegrationEvent;
