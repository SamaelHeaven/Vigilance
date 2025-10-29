using System.Numerics;

namespace Vigilance.Math;

public static class FloatingPointExtensions
{
    extension<T>(T value)
        where T : IFloatingPoint<T>
    {
        public T Round()
        {
            return T.Round(value);
        }

        public T Round(int digits)
        {
            return T.Round(value, digits);
        }

        public T Floor()
        {
            return T.Floor(value);
        }

        public T Ceil()
        {
            return T.Ceiling(value);
        }
    }

    extension(float value)
    {
        public float DegToRad()
        {
            return value * (MathF.PI / 180);
        }

        public float RadToDeg()
        {
            return value * (180 / MathF.PI);
        }

        public Vector2 DegToDirection()
        {
            return RadToDirection(value.DegToRad());
        }

        public Vector2 RadToDirection()
        {
            return new Vector2(MathF.Cos(value), MathF.Sin(value));
        }
    }

    extension(double value)
    {
        public double DegToRad()
        {
            return value * (System.Math.PI / 180);
        }

        public double RadToDeg()
        {
            return value * (180 / System.Math.PI);
        }

        public Vector2 DegToDirection()
        {
            return RadToDirection(value.DegToRad());
        }

        public Vector2 RadToDirection()
        {
            return new Vector2((float)System.Math.Cos(value), (float)System.Math.Sin(value));
        }
    }
}
