namespace Vigilance.Math;

public static class DoubleExtensions
{
    public static double DegToRad(this double degrees)
    {
        return degrees * (System.Math.PI / 180);
    }

    public static double RadToDeg(this double radians)
    {
        return radians * (180 / System.Math.PI);
    }

    public static Vector2 DirectionDeg(this double degrees)
    {
        return DirectionRad(degrees.DegToRad());
    }

    public static Vector2 DirectionRad(this double radians)
    {
        return new Vector2((float)System.Math.Cos(radians), (float)System.Math.Sin(radians));
    }

    public static double Min(this double value, double min)
    {
        return System.Math.Min(value, min);
    }

    public static double Max(this double value, double max)
    {
        return System.Math.Max(value, max);
    }

    public static double Clamp(this double value, double min, double max)
    {
        return System.Math.Min(System.Math.Max(value, min), max);
    }

    public static double Round(this double value)
    {
        return System.Math.Round(value);
    }

    public static double Floor(this double value)
    {
        return System.Math.Floor(value);
    }

    public static double Ceil(this double value)
    {
        return System.Math.Ceiling(value);
    }

    public static double Abs(this double value)
    {
        return System.Math.Abs(value);
    }

    public static double Or(this double value, double defaultValue)
    {
        return value == 0 ? defaultValue : value;
    }
}
