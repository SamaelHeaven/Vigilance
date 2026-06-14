namespace Vigilance.Core;

public record struct Child : IWriteImmutableComponent, ISkipSetEventIfEqualComponent
{
    internal ulong NextSiblingId;
    internal ulong PreviousSiblingId;

    public Child(ulong parentId)
    {
        ParentId = parentId;
    }

    public ulong ParentId { get; set; }

    public bool Equals(Child other)
    {
        return ParentId == other.ParentId;
    }

    public override readonly int GetHashCode()
    {
        return ParentId.GetHashCode();
    }
}
