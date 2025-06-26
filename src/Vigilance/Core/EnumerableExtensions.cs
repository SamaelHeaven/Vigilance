using System.Runtime.InteropServices;

namespace Vigilance.Core;

public static class EnumerableExtensions
{
    public static ReadOnlySpan<T> AsSpan<T>(this IEnumerable<T> enumerable)
    {
        return enumerable switch
        {
            T[] array => array,
            List<T> list => CollectionsMarshal.AsSpan(list),
            _ => enumerable.ToArray(),
        };
    }
}
