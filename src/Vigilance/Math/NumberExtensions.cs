using System.Numerics;

namespace Vigilance.Math;

public static class NumberExtensions
{
    extension<T>(T value)
        where T : INumber<T>
    {
        public int Sign()
        {
            return T.Sign(value);
        }

        public T Abs()
        {
            return T.Abs(value);
        }

        public T Min(T min)
        {
            return T.Min(value, min);
        }

        public T Max(T max)
        {
            return T.Max(value, max);
        }

        public T Clamp(T min, T max)
        {
            return T.Clamp(value, min, max);
        }

        public T Or(T defaultValue)
        {
            return value == default ? defaultValue : value;
        }
    }
}
