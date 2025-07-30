using Vigilance.Core;

namespace Vigilance.Drawing;

public sealed class CircleGradient : IFullCloneable
{
    public Color InnerFill { get; set; } = Color.White;
    public Color OuterFill { get; set; } = Color.White;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 0;
    public CameraProvider Camera { get; set; } = Core.Camera.Scene;

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
        return Printer.Print(this);
    }
}
