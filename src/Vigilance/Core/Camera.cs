using System.Numerics;
using Vigilance.Logging;
using Vigilance.Math;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.Core;

public sealed class Camera
{
    private Matrix3x2? _matrixCache = null;
    public static CameraProvider Null => default;
    public static CameraProvider Scene { get; } = new(() => Game.Scene.Camera);

    public Vector2 Target
    {
        get;
        set
        {
            if (Precision.AreEqual(field, value))
                return;
            field = value;
            _matrixCache = null;
        }
    } = Vector2.Zero;

    public Vector2 Offset
    {
        get;
        set
        {
            if (Precision.AreEqual(field, value))
                return;
            field = value;
            _matrixCache = null;
        }
    } = Vector2.Zero;

    public float Rotation
    {
        get;
        set
        {
            if (Precision.AreEqual(field, value))
                return;
            field = value;
            _matrixCache = null;
        }
    } = 0;

    public float Zoom
    {
        get;
        set
        {
            if (Precision.AreEqual(field, value))
                return;
            field = value;
            _matrixCache = null;
        }
    } = 1;

    public Matrix3x2 Matrix
    {
        get
        {
            if (_matrixCache.HasValue)
                return _matrixCache.Value;
            var pixelSize = 1 / Zoom;
            var target = (Target / pixelSize).Floor() * pixelSize;
            var offset = Offset;
            var originMatrix = Matrix3x2.CreateTranslation(-target.X, -target.Y);
            var rotationMatrix = Matrix3x2.CreateRotation(Rotation.DegToRad());
            var scaleMatrix = Matrix3x2.CreateScale(Zoom, Zoom);
            var translationMatrix = Matrix3x2.CreateTranslation(offset.X, offset.Y);
            _matrixCache = originMatrix * scaleMatrix * rotationMatrix * translationMatrix;
            return _matrixCache.Value;
        }
    }

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Matrix)));
    }
}

public readonly record struct CameraProvider(Func<Camera?>? Func)
{
    public Camera? Get()
    {
        return Func?.Invoke();
    }

    public override string? ToString()
    {
        return Get()?.ToString();
    }

    public static implicit operator Camera?(CameraProvider provider)
    {
        return provider.Get();
    }
}
