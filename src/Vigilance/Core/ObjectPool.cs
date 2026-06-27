using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vigilance.Collections;
using BindingFlags = System.Reflection.BindingFlags;

namespace Vigilance.Core;

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
public sealed unsafe class ObjectPool<
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors
    )]
        T
>
    where T : class, new()
{
    private static readonly T _uninitialized = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
    private static readonly nuint _size = GetRawObjectDataSize(null, _uninitialized);
    private static readonly delegate* <T, void> _constructor;
    private ValueStack<T> _pool = [];

    static ObjectPool()
    {
        var constructor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.Public, [])!;
        _constructor = (delegate* <T, void>)constructor.MethodHandle.GetFunctionPointer();
    }

    public static ObjectPool<T> Shared { get; } = new();

    public int Count => _pool.Count;

    public int Capacity
    {
        get => _pool.Capacity;
        set => _pool.Capacity = value;
    }

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern ref byte GetRawData(
        [UnsafeAccessorType("System.Runtime.CompilerServices.RuntimeHelpers")] object? clazz,
        object obj
    );

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

    public T Rent()
    {
        if (!_pool.TryPop(out var item))
            return new T();
        _constructor(item);
        return item;
    }

    public Handle Borrow()
    {
        return new Handle(this, Rent());
    }

    public void Return(T item)
    {
        Debug.Assert(item is not null);
        if ((T?)item is null)
            return;
        ref var data = ref GetRawData(null, item);
        ref var uninitializedData = ref GetRawData(null, _uninitialized);
        BulkMoveWithWriteBarrier(null, ref data, ref uninitializedData, _size);
        _pool.Push(item);
    }

    public void Clear()
    {
        _pool.Clear();
        _pool.Capacity = 0;
    }

    public void TrimExcess()
    {
        _pool.TrimExcess();
    }

    public readonly ref struct Handle(ObjectPool<T> pool, T value) : IDisposable
    {
        public ObjectPool<T> Pool { get; } = pool;
        public T Value { get; } = value;

        public void Dispose()
        {
            Pool.Return(Value);
        }

        public static implicit operator T(Handle handle)
        {
            return handle.Value;
        }
    }
}
