using Vigilance.Core;

namespace Vigilance.Math;

public static class Coordinates
{
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
