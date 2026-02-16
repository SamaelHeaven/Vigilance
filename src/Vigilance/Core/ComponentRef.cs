using System.Runtime.CompilerServices;

namespace Vigilance.Core;

public readonly ref struct ComponentRef<T>
{
    private readonly ref T _value;

    public ComponentRef(ref T value)
    {
        _value = ref value;
    }

    public static ComponentRef<T> Null => new(ref Unsafe.NullRef<T>());

    public static bool WriteImmutable { get; } = typeof(IWriteImmutableComponent).IsAssignableFrom(typeof(T));

    public bool IsNull => Unsafe.IsNullRef(ref _value);

    public ref readonly T Read => ref _value;

    public ref T Write
    {
        get
        {
            if (!IsNull && WriteImmutable)
                throw new InvalidOperationException(
                    $"Cannot write {typeof(T)} because it implements {nameof(IWriteImmutableComponent)}."
                );
            return ref _value;
        }
    }

    public static implicit operator T(ComponentRef<T> componentRef)
    {
        return componentRef._value;
    }
}
