using System.Numerics;
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

    public Matrix4x4 Matrix
    {
        get
        {
            var camera = new Camera2D(Offset, Target, Rotation, Zoom);
            return Raylib.GetCameraMatrix2D(camera);
        }
    }

    public Camera() { }
}
