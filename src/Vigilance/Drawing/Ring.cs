using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public struct Ring
{
    public Vector2 Center = Vector2.Zero;
    public float InnerRadius = 0;
    public float OuterRadius = 0;
    public float StartAngle = 0;
    public float EndAngle = 360;
    public Color Fill = Color.Transparent;
    public Color Stroke = Color.Transparent;
    public float StrokeWidth = 0;
    public Func<Camera>? Camera = () => Game.Scene.Camera;

    public Ring() { }
}
