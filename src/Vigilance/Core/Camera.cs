using System.Numerics;
using Vigilance.Math;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.Core;

public delegate Camera CameraFunc();

public sealed class Camera
{
    public static CameraFunc Default { get; } = () => Game.Scene.Camera;
    public Vector2 Target { get; set; } = Vector2.Zero;
    public Vector2 Offset { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0;
    public float Zoom { get; set; } = 1;

    public Matrix3x2 Matrix
    {
        get
        {
            var target = Target.Round();
            var offset = Offset.Round();
            var originMatrix = Matrix3x2.CreateTranslation(-target.X, -target.Y);
            var rotationMatrix = Matrix3x2.CreateRotation(Rotation.DegToRad());
            var scaleMatrix = Matrix3x2.CreateScale(Zoom, Zoom);
            var translationMatrix = Matrix3x2.CreateTranslation(offset.X, offset.Y);
            return originMatrix * scaleMatrix * rotationMatrix * translationMatrix;
        }
    }

    public static implicit operator Camera?(CameraFunc? func)
    {
        return func?.Invoke();
    }
}
