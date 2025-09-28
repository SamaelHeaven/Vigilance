namespace Vigilance.Core;

public readonly record struct Wrapper<T>(T Value) : IEquatable<T>
{
    public static implicit operator Wrapper<T>(T t) => new(t);

    public static implicit operator T(Wrapper<T> source) => source.Value;

    public bool Equals(T? other)
    {
        return Value?.Equals(other) ?? false;
    }

    public override string? ToString()
    {
        return Value?.ToString();
    }
}
