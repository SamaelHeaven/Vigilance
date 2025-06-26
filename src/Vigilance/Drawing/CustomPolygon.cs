using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class CustomPolygon
{
    public CustomPolygon() { }

    public CustomPolygon(IReadOnlyList<Vector2> points)
    {
        Points = points;
    }

    public IReadOnlyList<Vector2> Points { get; set; } = Array.Empty<Vector2>();
    public Color Fill { get; set; } = Color.White;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 0;
    public CameraFunc? Camera { get; set; } = Core.Camera.Default;
}
