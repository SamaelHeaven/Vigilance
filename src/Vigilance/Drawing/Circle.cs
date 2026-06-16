using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Circle : Drawable<Circle>, IFullCloneable
{
    public Circle() { }

    public Circle(Color fill)
    {
        Fill = fill;
    }

    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public float StartAngle { get; set; } = 0;
    public float EndAngle { get; set; } = 360;
    public int Segments { get; set; } = 0;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform)), true);
    }

    public override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawCircle(transform, this);
    }
}
