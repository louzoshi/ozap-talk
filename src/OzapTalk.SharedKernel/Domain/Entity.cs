namespace OzapTalk.SharedKernel.Domain;

/// <summary>Base for all persistent entities. Ids are ULID-ish GUIDv7 strings created by the app, never the database.</summary>
public abstract class Entity
{
    protected Entity(Guid id) => Id = id;

    // EF Core
    protected Entity() { }

    public Guid Id { get; protected init; }

    public override bool Equals(object? obj) =>
        obj is Entity other && GetType() == other.GetType() && Id.Equals(other.Id);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
