using System.Numerics;
using Raylib_cs;
using Vigilance.Math;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.Core;

public delegate Camera CameraFunc();

public sealed class Camera
{
    public static CameraFunc DefaultFunc { get; } = () => Game.Scene.Camera;
    public Vector2 Target { get; set; } = Vector2.Zero;
    public Vector2 Offset { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0;
    public float Zoom { get; set; } = 1;

    public Matrix4x4 Matrix
    {
        get
        {
            var originMatrix = Matrix4x4.CreateTranslation(-Target.X, -Target.Y, 0);
            var rotationMatrix = Matrix4x4.CreateRotationZ(Rotation.DegToRad());
            var scaleMatrix = Matrix4x4.CreateScale(new Vector3(Zoom, Zoom, 1));
            var translationMatrix = Matrix4x4.CreateTranslation(Offset.X, Offset.Y, 0);
            return originMatrix * scaleMatrix * rotationMatrix * translationMatrix;
        }
    }

    internal Camera2D RCamera => new(Offset, Target, Rotation, Zoom);
}
