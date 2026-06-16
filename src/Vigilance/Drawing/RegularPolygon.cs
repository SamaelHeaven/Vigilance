using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class RegularPolygon : Drawable<RegularPolygon>, IFullCloneable
{
    public RegularPolygon() { }

    public RegularPolygon(Color fill)
    {
        Fill = fill;
    }

    public RegularPolygon(int sides)
    {
        Sides = sides;
    }

    public RegularPolygon(int sides, Color fill)
        : this(sides)
    {
        Fill = fill;
    }

    public int Sides { get; set; } = 0;
    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform)), true);
    }

    public override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawRegularPolygon(transform, this);
    }
}
