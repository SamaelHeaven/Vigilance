using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Drawing;

public sealed class CustomPolygon : Drawable<CustomPolygon>, IFullCloneable
{
    public CustomPolygon() { }

    public CustomPolygon(Color fill)
    {
        Fill = fill;
    }

    public CustomPolygon(IEnumerable<Vector2> points)
    {
        Points = points.ToList();
    }

    public CustomPolygon(IEnumerable<Vector2> points, Color fill)
        : this(points)
    {
        Fill = fill;
    }

    public CustomPolygon(List<Vector2> points)
    {
        Points = points;
    }

    public CustomPolygon(List<Vector2> points, Color fill)
        : this(points)
    {
        Fill = fill;
    }

    public List<Vector2> Points { get; set; } = [];
    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    object IDeepCloneable.DeepClone()
    {
        var result = this.ShallowClone();
        result.Points = Points.AsValueEnumerable().ToList();
        return result;
    }

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform)), true);
    }

    public override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawCustomPolygon(transform, this);
    }
}
