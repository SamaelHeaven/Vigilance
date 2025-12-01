using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vigilance.Collections;

namespace Vigilance.Core;

public static class SpanExtensions
{
    [OverloadResolutionPriority(-1)]
    public static ReadOnlySpan<T> AsSpan<T>(this IEnumerable<T> enumerable)
    {
        return enumerable switch
        {
            T[] array => array,
            List<T> list => CollectionsMarshal.AsSpan(list),
            string str when typeof(T) == typeof(char) => MemoryMarshal.CreateReadOnlySpan(
                ref Unsafe.As<char, T>(ref MemoryMarshal.GetReference(MemoryExtensions.AsSpan(str))),
                str.Length
            ),
            ArraySegment<T> segment => MemoryExtensions.AsSpan(segment),
            ISpanView<T> span => span.AsSpan(),
            _ => enumerable.ToArray(),
        };
    }

    public static Span<T> AsSpan<T>(this List<T> list)
    {
        return CollectionsMarshal.AsSpan(list);
    }
}
