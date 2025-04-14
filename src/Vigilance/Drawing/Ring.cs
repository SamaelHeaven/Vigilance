using Vigilance.Core;

namespace Vigilance.Drawing;

public struct Ring
{
    public float InnerRadius = 0;
    public float OuterRadius = 0;
    public float StartAngle = 0;
    public float EndAngle = 360;
    public Color Fill = Color.Transparent;
    public Color Stroke = Color.Transparent;
    public float StrokeWidth = 0;
    public CameraProvider? Camera = Core.Camera.DefaultProvider;

    public Ring() { }
}
