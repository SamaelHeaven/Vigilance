using Vigilance.Math;

namespace Vigilance.Core;

internal record struct PivotPoint(Vector2 Value)
    : IWriteImmutableComponent,
        IRemoveImmutableComponent,
        ISkipAddEventComponent,
        ISkipRemoveEventComponent
{
    public PivotPoint()
        : this(Vector2.Zero) { }

    public static implicit operator Vector2(PivotPoint pivotPoint)
    {
        return pivotPoint.Value;
    }

    public static implicit operator PivotPoint(Vector2 pivotPoint)
    {
        return new PivotPoint(pivotPoint);
    }
}
