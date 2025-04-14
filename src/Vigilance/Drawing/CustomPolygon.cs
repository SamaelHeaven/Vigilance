using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public struct CustomPolygon
{
    public ICollection<Vector2> Points = [];
    public Color Fill = Color.Transparent;
    public Color Stroke = Color.Transparent;
    public float StrokeWidth = 0;
    public CameraProvider? Camera = Core.Camera.DefaultProvider;

    public CustomPolygon() { }
}
