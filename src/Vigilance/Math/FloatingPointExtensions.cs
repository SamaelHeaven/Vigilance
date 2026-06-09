using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vigilance.Math;

public static class FloatingPointExtensions
{
    extension<T>(T value)
        where T : IFloatingPoint<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Round()
        {
            return T.Round(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Round(int digits)
        {
            return T.Round(value, digits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Round(MidpointRounding mode)
        {
            return T.Round(value, mode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Round(int digits, MidpointRounding mode)
        {
            return T.Round(value, digits, mode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Floor()
        {
            return T.Floor(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Ceil()
        {
            return T.Ceiling(value);
        }
    }

    extension(float value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DegToRad()
        {
            return value * (MathF.PI / 180);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float RadToDeg()
        {
            return value * (180 / MathF.PI);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 DegToDirection()
        {
            return value.DegToRad().RadToDirection();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 RadToDirection()
        {
            return new Vector2(MathF.Cos(value), MathF.Sin(value));
        }
    }

    extension(double value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double DegToRad()
        {
            return value * (System.Math.PI / 180);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double RadToDeg()
        {
            return value * (180 / System.Math.PI);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 DegToDirection()
        {
            return value.DegToRad().RadToDirection();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 RadToDirection()
        {
            return new Vector2((float)System.Math.Cos(value), (float)System.Math.Sin(value));
        }
    }
}
