using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vigilance.Math;

public static class NumberExtensions
{
    extension<T>(T value)
        where T : INumber<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Sign()
        {
            return T.Sign(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Abs()
        {
            return T.Abs(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Min(T min)
        {
            return T.Min(value, min);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Max(T max)
        {
            return T.Max(value, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Clamp(T min, T max)
        {
            return T.Clamp(value, min, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Or(T defaultValue)
        {
            return value == default ? defaultValue : value;
        }
    }
}
