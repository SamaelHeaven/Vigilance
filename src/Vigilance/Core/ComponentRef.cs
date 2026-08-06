using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Vigilance.Core;

public readonly ref struct ComponentRef<T>
{
    internal readonly ref T Value;
    public int Index { get; }

    public ComponentRef(ref T value, int index)
    {
        Value = ref value;
        Index = index;
    }

    public static ComponentRef<T> Null => new(ref Unsafe.NullRef<T>(), -1);

    public static bool WriteImmutable => typeof(IWriteImmutableComponent).IsAssignableFrom(typeof(T));

    public bool IsNull => Unsafe.IsNullRef(ref Value);

    public bool CanWrite => !typeof(IWriteImmutableComponent).IsAssignableFrom(typeof(T));

    public ref readonly T Read => ref Value;

    public ref T Write
    {
        get
        {
            if (WriteImmutable)
                throw new InvalidOperationException(
                    $"Cannot write {typeof(T)} because it implements {nameof(IWriteImmutableComponent)}."
                );
            return ref Value;
        }
    }

    public static implicit operator T(ComponentRef<T> componentRef)
    {
        return componentRef.Value;
    }

    public void Deconstruct(out T value, out int index)
    {
        value = Value;
        index = Index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? GetOrDefault()
    {
        return IsNull ? default : Read;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetOrDefault(in T defaultValue)
    {
        return IsNull ? defaultValue : Read;
    }

    public WritableComponentRef<T> AsWritable()
    {
        return new WritableComponentRef<T>(this);
    }
}

public ref struct WritableComponentRef<T>(scoped in ComponentRef<T> componentRef)
{
    private readonly ComponentRef<T> _componentRef = componentRef;
    private T _value = default!;

    [UnscopedRef]
    public ref T Value
    {
        get
        {
            if (!ComponentRef<T>.WriteImmutable)
                return ref _componentRef.Write;
            _value = _componentRef.Read;
            return ref _value;
        }
    }
}
