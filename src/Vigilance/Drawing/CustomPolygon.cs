using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public struct CustomPolygon
{
    public IReadOnlyList<Vector2> Points { get; set; } = Array.Empty<Vector2>();
    public Color Fill { get; set; } = Color.Transparent;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 0;
    public CameraProvider? Camera { get; set; } = Core.Camera.DefaultProvider;

    public CustomPolygon() { }
}
