using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Triangle
{
    public Triangle() { }

    public Triangle(Vector2 v1, Vector2 v2, Vector2 v3)
    {
        V1 = v1;
        V2 = v2;
        V3 = v3;
    }

    public Vector2 V1 { get; set; } = Vector2.Zero;
    public Vector2 V2 { get; set; } = Vector2.Zero;
    public Vector2 V3 { get; set; } = Vector2.Zero;
    public Color Fill { get; set; } = Color.White;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 1;
    public CameraFunc? Camera { get; set; } = Core.Camera.DefaultFunc;
}
