using Vigilance.Math;

namespace Vigilance.Core;

public delegate Camera CameraProvider();

public struct Camera
{
    public static readonly CameraProvider DefaultProvider = () => Game.Scene.Camera;
    public Vector2 Target { get; set; } = Vector2.Zero;
    public Vector2 Offset { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0;
    public float Zoom { get; set; } = 1;

    public Camera() { }
}
