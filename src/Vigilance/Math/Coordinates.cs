using Vigilance.Core;

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

    public static Vector2 ScreenToLocal(Vector2 coordinates)
    {
        var size = Game.Size;
        var screenSize = Game.ScreenSize;
        var scale = MathF.Min(screenSize.X / size.X, screenSize.Y / size.Y);
        coordinates -= (screenSize - size * scale) * 0.5f;
        coordinates /= scale;
        return coordinates;
    }

    public static Vector2 LocalToScreen(Vector2 coordinates)
    {
        var size = Game.Size;
        var screenSize = Game.ScreenSize;
        var scale = MathF.Min(screenSize.X / size.X, screenSize.Y / size.Y);
        coordinates *= scale;
        coordinates += (screenSize - size * scale) * 0.5f;
        return coordinates;
    }
}
