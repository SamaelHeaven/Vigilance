using System.Runtime.CompilerServices;

namespace Vigilance.Core;

public readonly ref struct ComponentRef<T>
{
    internal readonly ref T Value;

    public ComponentRef(ref T value)
    {
        Value = ref value;
    }

    public static ComponentRef<T> Null => new(ref Unsafe.NullRef<T>());

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
}
