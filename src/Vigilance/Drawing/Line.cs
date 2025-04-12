using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public struct Line
{
    public Vector2 Start = Vector2.Zero;
    public Vector2 End = Vector2.Zero;
    public Color Color = Color.Transparent;
    public float Thickness = 1;
    public Func<Camera>? Camera = () => Game.Scene.Camera;

    public Line() { }
}
