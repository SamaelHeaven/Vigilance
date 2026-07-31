using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Vigilance.Core;

public static class ObjectMarshal
{
    private static ref byte GetRawData(object obj)
    {
        return ref Unsafe.As<RawData>(obj).Data;
    }

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern nuint GetRawObjectDataSize(
        [UnsafeAccessorType("System.Runtime.CompilerServices.RuntimeHelpers")] object? clazz,
        object obj
    );

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern void BulkMoveWithWriteBarrier(
        [UnsafeAccessorType("System.Buffer")] object? clazz,
        ref byte destination,
        ref byte source,
        nuint byteCount
    );

    public static int SizeOf(object value)
    {
        return (int)GetRawObjectDataSize(null, value);
    }

    [SkipLocalsInit]
    public static unsafe void Clear(object value)
    {
        var size = (int)GetRawObjectDataSize(null, value);
        byte[]? pooledArray = null;
        try
        {
            Span<byte> span;
            if (size > 1024)
            {
                pooledArray = ArrayPool<byte>.Shared.Rent(size);
                span = pooledArray.AsSpan(0, size);
            }
            else
            {
                var bytes = stackalloc byte[size];
                span = new Span<byte>(bytes, size);
            }

            span.Clear();
            ref var data = ref GetRawData(value);
            BulkMoveWithWriteBarrier(null, ref data, ref span.GetPinnableReference(), (nuint)size);
        }
        finally
        {
            if (pooledArray is not null)
                ArrayPool<byte>.Shared.Return(pooledArray);
        }
    }

    public static void Write(object source, object dest)
    {
        var size = GetRawObjectDataSize(null, dest);
        Debug.Assert(size == GetRawObjectDataSize(null, source));
        ref var sourceData = ref GetRawData(source);
        ref var destData = ref GetRawData(dest);
        BulkMoveWithWriteBarrier(null, ref destData, ref sourceData, size);
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private sealed class RawData
    {
        public byte Data;
    }
}
