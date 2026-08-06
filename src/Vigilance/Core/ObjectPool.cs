using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Vigilance.Core;

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
public sealed unsafe class ObjectPool<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>
    where T : class, new()
{
    private static readonly delegate* <T, void> _constructor;

    [ThreadStatic]
    private static ObjectPool<T>? _shared;

    private bool _objectMarshalUnavailable;
    private ValueStack<T> _pool = [];

    static ObjectPool()
    {
        var constructor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.Public, []);
        _constructor = constructor is null ? null : (delegate* <T, void>)constructor.MethodHandle.GetFunctionPointer();
    }

    public static ObjectPool<T> Shared => _shared ??= new ObjectPool<T>();

    public int Count => _pool.Count;

    public int Capacity
    {
        get => _pool.Capacity;
        set => _pool.Capacity = value;
    }

    public T Rent()
    {
        if (!_pool.TryPop(out var item))
            return new T();
        if (_constructor is not null)
            _constructor(item);
        return item;
    }

    public void Return(T item)
    {
        Debug.Assert(item is not null);
        if ((T?)item is null || _objectMarshalUnavailable)
            return;
        try
        {
            ObjectMarshal.Clear(item);
            _pool.Push(item);
        }
        catch (Exception e)
        {
            Log.Error(e);
            _objectMarshalUnavailable = true;
        }
    }

    public Handle Borrow()
    {
        return new Handle(this, Rent());
    }

    public void Clear()
    {
        _pool.Clear();
    }

    public void EnsureCapacity(int capacity)
    {
        _pool.EnsureCapacity(capacity);
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
