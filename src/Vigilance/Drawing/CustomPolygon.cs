using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class CustomPolygon : IFullCloneable
{
    public CustomPolygon() { }

    public CustomPolygon(IReadOnlyList<Vector2> points)
    {
        Points = points;
    }

    public CustomPolygon(IReadOnlyList<Vector2> points, Color fill)
        : this(points)
    {
        Fill = fill;
    }

    public IReadOnlyList<Vector2> Points { get; set; } = Array.Empty<Vector2>();
    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;

    object IDeepCloneable.DeepClone()
    {
        var result = this.ShallowClone();
        result.Points = Points.ToArray();
        return result;
    }

    public override string ToString()
    {
        return ObjectPrinter.Print(this);
    }
}
