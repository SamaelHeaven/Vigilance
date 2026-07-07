using Box2D.NET;

namespace Vigilance.Physics;

public record struct ShapeFilter
{
    public ShapeFilter()
    {
        Category = ShapeFilterCategory.DefaultCategory;
        Mask = ShapeFilterCategory.DefaultMask;
    }

    public ShapeFilterCategory Category { get; set; }
    public ShapeFilterCategory Mask { get; set; }

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
    DefaultCategory = 1,
    DefaultMask = ulong.MaxValue,
}
