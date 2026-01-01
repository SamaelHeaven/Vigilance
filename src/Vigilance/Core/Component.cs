using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Flecs.NET.Core;

namespace Vigilance.Core;

public readonly unsafe record struct Component
{
    internal Component(ulong id, Scene scene, Type type)
    {
        Id = id;
        Scene = scene;
        Type = type;
        Metadata = ComponentMetadata.Map[type];
    }

    public ulong Id { get; }
    public Scene Scene { get; }
    public Type Type { get; }
    public ComponentMetadata Metadata { get; }

    public bool IsNull => Id == 0;

    public bool Equals(Component other)
    {
        return Id == other.Id && Scene == other.Scene;
    }

    public static ref T FromPointer<T>(nint ptr)
    {
        if (ptr == 0)
            return ref Unsafe.NullRef<T>();
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return ref Unsafe.AsRef<T>((void*)ptr);
        var handle = GCHandle.FromIntPtr(*(nint*)ptr);
        var box = (StrongBox<T>)handle.Target!;
        return ref box.Value!;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Scene);
    }

    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append("Id = ");
        builder.Append(Id);
        builder.Append(", Type = ");
        builder.Append(Type);
        return true;
    }
}

public sealed class ComponentMetadata
{
    internal static readonly Dictionary<Type, ComponentMetadata> Map = new();

    public required Type Type { get; init; }

    public required bool IsManaged { get; init; }

    public required bool IsTag { get; init; }

    public required int Size { get; init; }

    public required int Alignment { get; init; }

    public required Func<Scene, ulong> IdFunc { get; init; }

    public required Func<object?> DefaultFunc { get; init; }

    public required Func<nint, object?> FromPointerFunc { get; init; }

    public required Action<Entity, object?> SetAction { get; init; }
}

public unsafe struct ComponentMetadata<T>
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
                IdFunc = scene => Type<T>.Id(scene.World),
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
