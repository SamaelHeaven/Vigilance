using ZLinq;

namespace Vigilance.Collections;

public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> enumerable)
    {
        public int FindIndex(Func<T, bool> match)
        {
            var index = 0;
            foreach (var item in enumerable)
            {
                if (match.Invoke(item))
                    return index;
                index++;
            }

            return -1;
        }
    }

    extension<TEnumerator, T>(ValueEnumerable<TEnumerator, T> enumerable)
        where TEnumerator : struct, IValueEnumerator<T>, allows ref struct
    {
        public int FindIndex(Func<T, bool> match)
        {
            var index = 0;
            foreach (var item in enumerable)
            {
                if (match.Invoke(item))
                    return index;
                index++;
            }

            return -1;
        }
    }
}
