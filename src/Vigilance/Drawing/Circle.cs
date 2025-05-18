using Vigilance.Core;

namespace Vigilance.Drawing;

public sealed class Circle
{
    public Color Fill { get; set; } = Color.Transparent;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 0;
    public GetCameraDelegate? Camera { get; set; } = Core.Camera.DefaultDelegate;
}
