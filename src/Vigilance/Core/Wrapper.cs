namespace Vigilance.Core;

public readonly record struct Wrapper<T>(T Value) : IEquatable<T>
{
    public bool Equals(T? other)
    {
        return Value?.Equals(other) ?? false;
    }

    public static implicit operator Wrapper<T>(T t)
    {
        return new Wrapper<T>(t);
    }

    public static implicit operator T(Wrapper<T> source)
    {
        return source.Value;
    }

    public override string? ToString()
    {
        return Value?.ToString();
    }
}
