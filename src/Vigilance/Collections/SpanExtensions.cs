using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZLinq.Internal;

namespace Vigilance.Collections;

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
            IReadOnlySpan<T> span => span.AsSpan(),
            _ => enumerable.ToArray(),
        };
    }

    public static Span<T> AsSpan<T>(this List<T> list)
    {
        return CollectionsMarshal.AsSpan(list);
    }

    public static ReadOnlySpan<T> AsSpan<T>(this IReadOnlySpan<T> view)
    {
        return view.AsSpan();
    }

    public static bool TryCopyTo<T>(this ReadOnlySpan<T> span, scoped Span<T> destination, Index offset)
    {
        if (!EnumeratorHelper.TryGetSlice(span, offset, destination.Length, out var slice))
            return false;
        slice.CopyTo(destination);
        return true;
    }

    extension<T>(ref T value)
        where T : struct
    {
        [OverloadResolutionPriority(-2)]
        public Span<T> AsSpan()
        {
            return MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in value), 1);
        }

        [OverloadResolutionPriority(-1)]
        public Span<TValue> AsSpan<TValue>(int length)
        {
            return MemoryMarshal.CreateSpan(ref Unsafe.As<T, TValue>(ref value), length);
        }
    }
}
