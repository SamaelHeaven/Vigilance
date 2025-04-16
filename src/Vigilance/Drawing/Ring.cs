using Vigilance.Core;

namespace Vigilance.Drawing;

public struct Ring
{
    public float InnerRadius { get; set; } = 0;
    public float OuterRadius { get; set; } = 0;
    public float StartAngle { get; set; } = 0;
    public float EndAngle { get; set; } = 360;
    public Color Fill { get; set; } = Color.Transparent;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 0;
    public CameraProvider? Camera { get; set; } = Core.Camera.DefaultProvider;

    public Ring() { }
}
