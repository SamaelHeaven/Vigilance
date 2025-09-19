using Vigilance.Core;
using Vigilance.Logging;

namespace Vigilance.Drawing;

public sealed class RegularPolygon : IFullCloneable
{
    public RegularPolygon() { }

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
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;

    public override string ToString()
    {
        return ObjectPrinter.Print(this);
    }
}
