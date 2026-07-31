using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZLinq.Internal;

namespace Vigilance.Collections;

public interface IReadOnlySpan<TValue>
{
    ReadOnlySpan<TValue> AsSpan();
}

public static class SpanExtensions
{
    [OverloadResolutionPriority(-1)]
    public static ReadOnlySpan<T> AsSpan<T>(this IEnumerable<T> enumerable)
    {
        return enumerable switch
        {
            T[] array => array,
            List<T> list => CollectionsMarshal.AsSpan(list),
            IReadOnlySpan<T> span => span.AsSpan(),
            _ => enumerable.ToArray(),
        };
    }

    public static Span<T> AsSpan<T>(this T[] array)
    {
        return array;
    }

    public static Span<T> AsSpan<T>(this List<T> list)
    {
        return CollectionsMarshal.AsSpan(list);
    }

    public static bool TryCopyTo<T>(in this ReadOnlySpan<T> span, scoped Span<T> destination, Index offset)
    {
        if (!EnumeratorHelper.TryGetSlice(span, offset, destination.Length, out var slice))
            return false;
        slice.CopyTo(destination);
        return true;
    }
}
