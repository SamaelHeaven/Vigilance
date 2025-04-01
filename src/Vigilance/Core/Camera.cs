using Vigilance.Math;

namespace Vigilance.Core;

public struct Camera
{
    public Vector2 Target = Vector2.Zero;
    public Vector2 Offset = Vector2.Zero;
    public float Rotation = 0;
    public float Zoom = 1;

    public Camera() { }
}
