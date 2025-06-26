using System.Numerics;
using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Math;

public static class Coordinates
{
    public static Vector2 GetCenter(IReadOnlyCollection<Vector2> points)
    {
        return points.Aggregate(Vector2.Zero, (a, b) => a + b) / points.Count;
    }

    public static Vector2[] Scale(IReadOnlyCollection<Vector2> points, Vector2 scale, Vector2? offset = null)
    {
        scale = scale.Abs();
        var center = GetCenter(points);
        return points.Select(point => (offset ?? Vector2.Zero) + (center + (point - center) * scale)).ToArray();
    }

    public static Vector2 ScreenToLocal(Vector2 coordinates, Viewport? viewport = null)
    {
        var size = Game.Size;
        var screenSize = Game.ScreenSize;
        var scaleX = screenSize.X / size.X;
        var scaleY = screenSize.Y / size.Y;
        switch (viewport ?? Game.Viewport)
        {
            case Viewport.Fit:
            {
                var scale = MathF.Min(scaleX, scaleY);
                var offset = (screenSize - size * scale) * 0.5f;
                coordinates -= offset;
                coordinates /= scale;
                return coordinates;
            }
            case Viewport.Stretch:
            {
                coordinates.X /= scaleX;
                coordinates.Y /= scaleY;
                return coordinates;
            }
            case Viewport.Crop:
            {
                var scale = MathF.Max(scaleX, scaleY);
                var offset = (screenSize - size * scale) * 0.5f;
                coordinates -= offset;
                coordinates /= scale;
                return coordinates;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(viewport));
        }
    }

    public static Vector2 ScreenToWorld(Vector2 coordinates, Viewport? viewport = null, Camera? camera = null)
    {
        return LocalToWorld(ScreenToLocal(coordinates, viewport), camera);
    }

    public static Vector2 LocalToScreen(Vector2 coordinates, Viewport? viewport = null)
    {
        var size = Game.Size;
        var screenSize = Game.ScreenSize;
        var scaleX = screenSize.X / size.X;
        var scaleY = screenSize.Y / size.Y;
        switch (viewport ?? Game.Viewport)
        {
            case Viewport.Fit:
            {
                var scale = MathF.Min(scaleX, scaleY);
                var offset = (screenSize - size * scale) * 0.5f;
                coordinates *= scale;
                coordinates += offset;
                return coordinates;
            }
            case Viewport.Stretch:
            {
                coordinates.X *= scaleX;
                coordinates.Y *= scaleY;
                return coordinates;
            }
            case Viewport.Crop:
            {
                var scale = MathF.Max(scaleX, scaleY);
                var offset = (screenSize - size * scale) * 0.5f;
                coordinates *= scale;
                coordinates += offset;
                return coordinates;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(viewport));
        }
    }

    public static Vector2 LocalToWorld(Vector2 coordinates, Camera? camera = null)
    {
        camera ??= Game.Scene.Camera;
        return Matrix3x2.Invert(camera.Matrix, out var matrix) ? coordinates.Transform(matrix) : Vector2.Zero;
    }

    public static Vector2 WorldToLocal(Vector2 coordinates, Camera? camera = null)
    {
        camera ??= Game.Scene.Camera;
        return coordinates.Transform(camera.Matrix);
    }

    public static Vector2 WorldToScreen(Vector2 coordinates, Camera? camera = null, Viewport? viewport = null)
    {
        return LocalToScreen(WorldToLocal(coordinates, camera), viewport);
    }
}
