using Vigilance.Core;
using Vigilance.Logging;

namespace Vigilance.Drawing;

public sealed class CircleGradient : IFullCloneable
{
    public Color InnerFill { get; set; } = Drawing.DefaultFill;
    public Color OuterFill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public DrawingOrder DrawingOrder { get; set; } = Drawing.DefaultOrder;
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;

    public Color Fill
    {
        get => InnerFill.Blend(OuterFill);
        set
        {
            InnerFill = value;
            OuterFill = value;
        }
    }

    public override string ToString()
    {
        return ObjectPrinter.Print(this);
    }
}
