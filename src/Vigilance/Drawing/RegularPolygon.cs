using Vigilance.Core;

namespace Vigilance.Drawing;

public sealed class RegularPolygon
{
    public int Sides { get; set; } = 0;
    public Color Fill { get; set; } = Color.Transparent;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 0;
    public GetCameraDelegate? Camera { get; set; } = Core.Camera.DefaultDelegate;
}
