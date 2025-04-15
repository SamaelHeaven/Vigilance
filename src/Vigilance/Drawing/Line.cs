using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public struct Line
{
    public Vector2 Start { get; set; } = Vector2.Zero;
    public Vector2 End { get; set; } = Vector2.Zero;
    public Color Color { get; set; } = Color.Transparent;
    public float Thickness { get; set; } = 1;
    public CameraProvider? Camera { get; set; } = Core.Camera.DefaultProvider;

    public Line() { }
}
