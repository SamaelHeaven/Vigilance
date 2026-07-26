namespace Vigilance.Core;

public record struct Child : IWriteImmutableComponent, ISkipSetEventIfEqualComponent
{
    internal EntityId NextSiblingId;
    internal EntityId PreviousSiblingId;

    public Child(EntityId parentId)
    {
        ParentId = parentId;
    }

    public EntityId ParentId { get; set; }

    public bool Equals(Child other)
    {
        return ParentId == other.ParentId;
    }

    public override readonly int GetHashCode()
    {
        return ParentId.GetHashCode();
    }
}
