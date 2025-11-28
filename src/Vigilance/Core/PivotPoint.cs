using Vigilance.Math;

namespace Vigilance.Core;

internal record struct PivotPoint(Vector2 Value)
{
    public PivotPoint()
        : this(Vector2.Zero) { }
}
