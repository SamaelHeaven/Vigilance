using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Line : IFullCloneable
{
    public Line() { }

    public Line(Vector2 start, Vector2 end)
    {
        Start = start;
        End = end;
    }

    public Line(Vector2 start, Vector2 end, Color color)
    {
        Start = start;
        End = end;
        Color = color;
    }

    public Vector2 Start { get; set; } = Vector2.Zero;
    public Vector2 End { get; set; } = Vector2.Zero;
    public Color Color { get; set; } = Color.White;
    public float Thick { get; set; } = 1;
    public CameraFunc? Camera { get; set; } = Core.Camera.Default;
}
