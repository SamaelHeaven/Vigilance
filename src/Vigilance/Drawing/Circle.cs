using Vigilance.Core;
using Vigilance.Logging;

namespace Vigilance.Drawing;

public sealed class Circle : IFullCloneable
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
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;

    public override string ToString()
    {
        return ObjectPrinter.Print(this);
    }
}
