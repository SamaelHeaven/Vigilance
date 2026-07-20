using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vigilance.Logging;

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
    private static readonly T _uninitialized = null!;

    static Object()
    {
        try
        {
            IsAvailable = IsTypeValid();
            if (!IsAvailable)
            {
                Log.Error($"'{typeof(T)}' is not supported for {typeof(Object<T>)}.");
                return;
            }

            _uninitialized = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
            Size = GetRawObjectDataSize(null, _uninitialized);
        }
        catch (Exception e)
        {
            IsAvailable = false;
            Log.Error(e);
        }

        if (IsAvailable)
            CopyTo(_uninitialized, _uninitialized);
    }

    public static nuint Size { get; }
    public static bool IsAvailable { get; private set; }

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

    public static bool Clear(T value)
    {
        return CopyTo(value, _uninitialized);
    }

    public static bool CopyTo(T source, T dest)
    {
        if (!IsAvailable)
            return false;
        if (source.GetType() != typeof(T) || dest.GetType() != typeof(T))
        {
            Log.Error($"{typeof(Object<T>)}.{nameof(CopyTo)} requires instances of exactly '{typeof(T)}'.");
            return false;
        }

        try
        {
            ref var sourceData = ref GetRawData(null, source);
            ref var destData = ref GetRawData(null, dest);
            BulkMoveWithWriteBarrier(null, ref destData, ref sourceData, Size);
            return true;
        }
        catch (Exception e)
        {
            IsAvailable = false;
            Log.Error(e);
            return false;
        }
    }

    private static bool IsTypeValid()
    {
        return typeof(T).IsClass
            && !typeof(T).IsAbstract
            && !typeof(T).IsInterface
            && !typeof(T).IsValueType
            && !typeof(T).IsByRefLike
            && !typeof(T).IsPointer
            && !typeof(T).IsByRef
            && !typeof(T).ContainsGenericParameters
            && !typeof(T).IsCOMObject;
    }
}
