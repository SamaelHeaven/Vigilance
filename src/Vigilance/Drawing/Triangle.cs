using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public struct Triangle
{
    public Vector2 V1 { get; set; } = Vector2.Zero;
    public Vector2 V2 { get; set; } = Vector2.Zero;
    public Vector2 V3 { get; set; } = Vector2.Zero;
    public Color Fill { get; set; } = Color.Transparent;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 1;
    public CameraProvider? Camera { get; set; } = Core.Camera.DefaultProvider;

    public Triangle() { }
}
