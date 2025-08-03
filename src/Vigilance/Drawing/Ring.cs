using Vigilance.Core;
using Vigilance.Logging;

namespace Vigilance.Drawing;

public sealed class Ring : IFullCloneable
{
    public float InnerRadius { get; set; } = 0;
    public float OuterRadius { get; set; } = 0;
    public float StartAngle { get; set; } = 0;
    public float EndAngle { get; set; } = 360;
    public Color Fill { get; set; } = Color.White;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 0;
    public CameraProvider Camera { get; set; } = Core.Camera.Scene;

    public override string ToString()
    {
        return ObjectPrinter.Print(this);
    }
}
