using Vigilance.Math;

namespace Vigilance.Core;

internal readonly record struct PivotPoint(Vector2 Value)
{
    public PivotPoint()
        : this(Vector2.Zero) { }
}
