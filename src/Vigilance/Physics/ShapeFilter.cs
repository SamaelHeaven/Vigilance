using Box2D.NET;

namespace Vigilance.Physics;

public record struct ShapeFilter
{
    public ShapeFilter()
    {
        Category = ShapeFilterCategory.Default;
        Mask = ShapeFilterMask.Default;
    }

    public ShapeFilterCategory Category { get; set; }
    public ShapeFilterMask Mask { get; set; }

    public int GroupIndex { get; set; }

    internal readonly B2Filter B2Filter =>
        new()
        {
            categoryBits = (ulong)Category,
            maskBits = (ulong)Mask,
            groupIndex = GroupIndex,
        };
}

[Flags]
public enum ShapeFilterCategory : ulong
{
    Default = 1,
}

[Flags]
public enum ShapeFilterMask : ulong
{
    Default = ulong.MaxValue,
}
