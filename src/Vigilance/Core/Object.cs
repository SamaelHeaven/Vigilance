using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Vigilance.Core;

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
public static class Object<
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors
    )]
        T
>
    where T : class
{
    private static readonly T _uninitialized = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
    public static nuint Size { get; } = GetRawObjectDataSize(null, _uninitialized);

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

    public static void Clear(T value)
    {
        CopyTo(value, _uninitialized);
    }

    public static void CopyTo(T source, T dest)
    {
        ref var sourceData = ref GetRawData(null, source);
        ref var destData = ref GetRawData(null, dest);
        BulkMoveWithWriteBarrier(null, ref destData, ref sourceData, Size);
    }
}
