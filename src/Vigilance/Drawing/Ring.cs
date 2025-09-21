using Vigilance.Core;
using Vigilance.Logging;

namespace Vigilance.Drawing;

public sealed class Ring : IFullCloneable
{
    public float InnerRadius { get; set; } = 0;
    public float OuterRadius { get; set; } = 0;
    public float StartAngle { get; set; } = 0;
    public float EndAngle { get; set; } = 360;
    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public DrawingOrder DrawingOrder { get; set; } = Drawing.DefaultOrder;
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;

    public override string ToString()
    {
        return ObjectPrinter.Print(this);
    }
}
