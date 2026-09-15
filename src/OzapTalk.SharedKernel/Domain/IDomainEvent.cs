namespace OzapTalk.SharedKernel.Domain;

/// <summary>
/// Raised inside an aggregate, handled in-process within the same module and transaction.
/// For cross-module communication use <see cref="Messaging.IIntegrationEvent"/> instead.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOnUtc { get; }
}
