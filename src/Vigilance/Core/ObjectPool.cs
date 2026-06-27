using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vigilance.Collections;

namespace Vigilance.Core;

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
public static class ObjectPool<
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors
    )]
    T
>
    where T : class, new()
{
    private static readonly T _default = new();
    private static readonly T _uninitialized = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
    private static ValueStack<T> _pool = [];
    private static readonly nuint _size = GetRawObjectDataSize(null, _default);

    public static int Count => _pool.Count;

    public static int Capacity
    {
        get => _pool.Capacity;
        set => _pool.Capacity = value;
    }

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern ref byte GetRawData(
        [UnsafeAccessorType("System.Runtime.CompilerServices.RuntimeHelpers")]
        object? type,
        object obj
    );

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern nuint GetRawObjectDataSize(
        [UnsafeAccessorType("System.Runtime.CompilerServices.RuntimeHelpers")]
        object? type,
        object obj
    );

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern void BulkMoveWithWriteBarrier(
        [UnsafeAccessorType("System.Buffer")] object? type,
        ref byte destination,
        ref byte source,
        nuint byteCount
    );

    public static T Rent()
    {
        if (_pool.Count == 0)
            return new T();
        var item = _pool.Pop();
        ref var data = ref GetRawData(null, item);
        ref var defaultData = ref GetRawData(null, _default);
        BulkMoveWithWriteBarrier(null, ref data, ref defaultData, _size);
        return item;
    }

    public static void Return(T item)
    {
        Debug.Assert(item is not null);
        if ((T?)item is null)
            return;
        ref var data = ref GetRawData(null, item);
        ref var uninitializedData = ref GetRawData(null, _uninitialized);
        BulkMoveWithWriteBarrier(null, ref data, ref uninitializedData, _size);
        _pool.Push(item);
    }

    public static void Clear()
    {
        _pool.Clear();
        _pool.Capacity = 0;
    }

    public static void TrimExcess()
    {
        _pool.TrimExcess();
    }
}
