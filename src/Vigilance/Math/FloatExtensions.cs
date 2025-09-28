namespace Vigilance.Math;

public static class FloatExtensions
{
    public static float DegToRad(this float degrees)
    {
        return degrees * (MathF.PI / 180);
    }

    public static float RadToDeg(this float radians)
    {
        return radians * (180 / MathF.PI);
    }

    public static Vector2 DegToDirection(this float degrees)
    {
        return RadToDirection(degrees.DegToRad());
    }

    public static Vector2 RadToDirection(this float radians)
    {
        return new Vector2(MathF.Cos(radians), MathF.Sin(radians));
    }

    public static float Min(this float value, float min)
    {
        return MathF.Min(value, min);
    }

    public static float Max(this float value, float max)
    {
        return MathF.Max(value, max);
    }

    public static float Clamp(this float value, float min, float max)
    {
        return MathF.Min(MathF.Max(value, min), max);
    }

    public static float Round(this float value)
    {
        return MathF.Round(value);
    }

    public static float Floor(this float value)
    {
        return MathF.Floor(value);
    }

    public static float Ceil(this float value)
    {
        return MathF.Ceiling(value);
    }

    public static float Abs(this float value)
    {
        return MathF.Abs(value);
    }

    public static float Or(this float value, float defaultValue)
    {
        return value == 0 ? defaultValue : value;
    }
}
