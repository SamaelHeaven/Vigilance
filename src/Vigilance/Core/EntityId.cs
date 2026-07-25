namespace Vigilance.Core;

public record struct EntityId(int Index, int Version)
{
    public static EntityId Null => default;

    public bool IsNull => Index == 0;

    public readonly bool Equals(EntityId other)
    {
        return Index == other.Index && Version == other.Version;
    }

    public override readonly int GetHashCode()
    {
        return Index;
    }
}
