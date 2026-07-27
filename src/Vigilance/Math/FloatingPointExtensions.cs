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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpAngle(float startDegrees, float endDegrees, float t)
        {
            var delta = (endDegrees - startDegrees) % 360f;
            switch (delta)
            {
                case > 180f:
                    delta -= 360f;
                    break;
                case < -180f:
                    delta += 360f;
                    break;
            }

            return startDegrees + delta * t;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double LerpAngle(double startDegrees, double endDegrees, double t)
        {
            var delta = (endDegrees - startDegrees) % 360;
            switch (delta)
            {
                case > 180:
                    delta -= 360;
                    break;
                case < -180:
                    delta += 360;
                    break;
            }

            return startDegrees + delta * t;
        }
    }
}
