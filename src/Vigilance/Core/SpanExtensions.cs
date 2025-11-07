using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vigilance.Core;

public static class SpanExtensions
{
    public static ReadOnlySpan<T> AsSpan<T>(this IEnumerable<T> enumerable)
    {
        return enumerable switch
        {
            T[] array => array,
            List<T> list => CollectionsMarshal.AsSpan(list),
            string str when typeof(T) == typeof(char) => MemoryMarshal.CreateReadOnlySpan(
                ref Unsafe.As<char, T>(ref MemoryMarshal.GetReference(str.AsSpan())),
                str.Length
            ),
            ArraySegment<T> segment => segment.AsSpan(),
            IReadOnlySpan<T> span => span.AsSpan(),
            _ => enumerable.ToArray(),
        };
    }

    public static ReadOnlySpan<char> AsSpan(this string str)
    {
        return MemoryExtensions.AsSpan(str);
    }

    public static Span<T> AsSpan<T>(this ArraySegment<T> segment)
    {
        return MemoryExtensions.AsSpan(segment);
    }

    public static Span<T> AsSpan<T>(this List<T> list)
    {
        return CollectionsMarshal.AsSpan(list);
    }
}
