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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T RoundUpToMultipleOf(T multiple)
        {
            multiple = T.Abs(multiple);
            if (multiple == T.Zero)
                return T.Zero;
            var remainder = value % multiple;
            if (remainder == T.Zero)
                return value;
            return value + (remainder > T.Zero ? multiple - remainder : -remainder);
        }
    }

    extension<T>(T value)
        where T : IBinaryInteger<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T RoundUpToPowerOf2()
        {
            if (value <= T.One)
                return T.One;
            var bitWidth = value.GetByteCount() * 8;
            var maxShift = T.One << (bitWidth - 1) < T.Zero ? bitWidth - 2 : bitWidth - 1;
            var shift = int.CreateChecked(T.Log2(value - T.One) + T.One);
            if (shift >= maxShift)
                return T.One << maxShift;
            return T.One << shift;
        }
    }
}
