using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Line : Drawable<Line>, IFullCloneable
{
    public Line() { }

    public Line(Color color)
    {
        Color = color;
    }

    public Line(Vector2 start, Vector2 end)
    {
        Start = start;
        End = end;
    }

    public Line(Vector2 start, Vector2 end, Color color)
    {
        Start = start;
        End = end;
        Color = color;
    }

    public Vector2 Start { get; set; } = Vector2.Zero;
    public Vector2 End { get; set; } = Vector2.Zero;
    public Color Color { get; set; } = Drawing.DefaultFill;
    public float Thick { get; set; } = Drawing.DefaultStrokeWidth == 0 ? 1 : Drawing.DefaultStrokeWidth;

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform)), true);
    }

    public override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawLine(transform, this);
    }
}
