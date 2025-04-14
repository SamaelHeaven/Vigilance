using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public struct CustomPolygon
{
    public IReadOnlyList<Vector2> Points = Array.Empty<Vector2>();
    public Color Fill = Color.Transparent;
    public Color Stroke = Color.Transparent;
    public float StrokeWidth = 0;
    public CameraProvider? Camera = Core.Camera.DefaultProvider;

    public CustomPolygon() { }
}
