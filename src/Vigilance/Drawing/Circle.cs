using Vigilance.Core;

namespace Vigilance.Drawing;

public struct Circle
{
    public Color Fill = Color.Transparent;
    public Color Stroke = Color.Transparent;
    public float StrokeWidth = 0;
    public CameraProvider? Camera = Core.Camera.DefaultProvider;

    public Circle() { }
}
