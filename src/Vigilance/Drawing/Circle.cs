using Vigilance.Core;

namespace Vigilance.Drawing;

public struct Circle
{
    public Color Fill { get; set; } = Color.Transparent;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 0;
    public CameraProvider? Camera { get; set; } = Core.Camera.DefaultProvider;

    public Circle() { }
}
