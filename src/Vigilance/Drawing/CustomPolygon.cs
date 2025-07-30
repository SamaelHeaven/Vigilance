using Vigilance.Core;
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
    public Color Fill { get; set; } = Color.White;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 0;
    public CameraProvider Camera { get; set; } = Core.Camera.Scene;

    object IDeepCloneable.DeepClone()
    {
        var result = this.ShallowClone();
        result.Points = Points.ToArray();
        return result;
    }

    public override string ToString()
    {
        return Printer.Print(this);
    }
}
