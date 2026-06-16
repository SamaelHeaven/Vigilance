using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Rectangle : Drawable<Rectangle>, IFullCloneable
{
    public Rectangle() { }

    public Rectangle(Color fill)
    {
        Fill = fill;
    }

    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public float Radius { get; set; } = Drawing.DefaultRadius;
    public int Segments { get; set; } = 0;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform)), true);
    }

    public override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawRectangle(transform, this);
    }
}
