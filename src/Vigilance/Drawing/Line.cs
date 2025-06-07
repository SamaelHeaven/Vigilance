using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Line
{
    public Vector2 Start { get; set; } = Vector2.Zero;
    public Vector2 End { get; set; } = Vector2.Zero;
    public Color Color { get; set; } = Color.White;
    public float Thick { get; set; } = 1;
    public GetCameraDelegate? Camera { get; set; } = Core.Camera.DefaultDelegate;
}
