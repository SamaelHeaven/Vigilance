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
            Span<T> span => span,
            ReadOnlySpan<T> span => span,
            _ => enumerable.ToArray(),
        };
    }
}
