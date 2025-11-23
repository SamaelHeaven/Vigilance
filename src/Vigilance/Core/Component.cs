using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Flecs.NET.Core;

namespace Vigilance.Core;

public readonly unsafe record struct Component
{
    internal Component(ulong id, Scene scene, Type type)
    {
        Id = id;
        Scene = scene;
        Type = type;
    }

    public ulong Id { get; }
    public Scene Scene { get; }
    public Type Type { get; }

    public ref readonly ComponentMetadata Metadata =>
        ref CollectionsMarshal.GetValueRefOrNullRef(ComponentMetadata.Map, Type);

    public static ref readonly T FromPointer<T>(nint ptr)
    {
        if (ptr == 0)
            return ref Unsafe.NullRef<T>();
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return ref Unsafe.AsRef<T>((void*)ptr);
        var handle = GCHandle.FromIntPtr(*(nint*)ptr);
        var box = (StrongBox<T>)handle.Target!;
        return ref box.Value!;
    }
}

public readonly struct ComponentMetadata
{
    internal static readonly Dictionary<Type, ComponentMetadata> Map = new();

    public required Type Type { get; init; }

    public required bool IsManaged { get; init; }

    public required bool IsTag { get; init; }

    public required int Size { get; init; }

    public required int Alignment { get; init; }

    public required Func<object?> DefaultFunc { get; init; }

    public required Func<nint, object?> FromPointerFunc { get; init; }

    public required Action<Entity, object?> SetAction { get; init; }
}

public struct ComponentMetadata<T>
{
    static ComponentMetadata()
    {
        var type = typeof(T);
        ComponentMetadata.Map.Add(
            type,
            new ComponentMetadata
            {
                Type = type,
                IsManaged = RuntimeHelpers.IsReferenceOrContainsReferences<T>(),
                IsTag = Type<T>.IsTag,
                Size = Type<T>.Size,
                Alignment = Type<T>.Alignment,
                DefaultFunc = () => default(T),
                FromPointerFunc = ptr =>
                {
                    ref readonly var value = ref Component.FromPointer<T>(ptr);
                    return Unsafe.IsNullRef(in value) ? null : value;
                },
                SetAction = (entity, value) => entity.Set<T>((T)value!),
            }
        );
    }

    public static void EnsureInitialized() { }
}
