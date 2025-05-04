using Raylib_cs;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.Core;

public delegate Camera CameraProvider();

public struct Camera
{
    public static CameraProvider DefaultProvider { get; } = () => Game.Scene.Camera;
    public Vector2 Target { get; set; } = Vector2.Zero;
    public Vector2 Offset { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0;
    public float Zoom { get; set; } = 1;

    internal Camera2D RCamera => new(Offset, Target, Rotation, Zoom);

    public Camera() { }
}
