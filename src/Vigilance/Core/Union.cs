// ReSharper disable once CheckNamespace

namespace System.Runtime.CompilerServices;

#if !NET11_0_OR_GREATER
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class UnionAttribute : Attribute;

public interface IUnion
{
    object? Value { get; }
}
#endif
