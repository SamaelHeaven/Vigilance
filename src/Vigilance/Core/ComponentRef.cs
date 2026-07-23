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

    public static bool WriteImmutable { get; } = typeof(IWriteImmutableComponent).IsAssignableFrom(typeof(T));

    public bool IsNull => Unsafe.IsNullRef(ref Value);

    public ref readonly T Read => ref Value;

    public ref T Write
    {
        get
        {
            if (!IsNull && WriteImmutable)
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
    public T? GetOrDefault(in T? defaultValue = default)
    {
        return IsNull ? defaultValue : Read;
    }
}
