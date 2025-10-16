using System.Runtime.InteropServices;
using ZLinq;

namespace Vigilance.Core;

public static class EnumerableExtensions
{
    public static ReadOnlySpan<T> AsSpan<T>(this IEnumerable<T> enumerable)
    {
        return enumerable switch
        {
            T[] array => array,
            List<T> list => list.AsSpan(),
            IReadOnlySpan<T> span => span.AsSpan(),
            _ => enumerable.AsValueEnumerable().ToArray(),
        };
    }

    public static Span<T> AsSpan<T>(this List<T> list)
    {
        return CollectionsMarshal.AsSpan(list);
    }
}
