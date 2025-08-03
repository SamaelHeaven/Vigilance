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
    public Color Fill { get; set; } = Color.White;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 0;
    public CameraProvider Camera { get; set; } = Core.Camera.Scene;

    public override string ToString()
    {
        return ObjectPrinter.Print(this);
    }
}
