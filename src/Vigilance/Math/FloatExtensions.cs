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
}
