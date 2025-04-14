using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public struct Triangle
{
    public Vector2 V1 = Vector2.Zero;
    public Vector2 V2 = Vector2.Zero;
    public Vector2 V3 = Vector2.Zero;
    public Color Fill = Color.Transparent;
    public Color Stroke = Color.Transparent;
    public float StrokeWidth = 1;
    public CameraProvider? Camera = Core.Camera.DefaultProvider;

    public Triangle() { }
}
