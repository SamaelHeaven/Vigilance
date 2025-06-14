namespace Vigilance.Core;

public readonly struct Component(Type type, object? data = null)
{
    public Type Type { get; } = type;
    public object? Data { get; } = data;

    public override bool Equals(object? obj)
    {
        return obj is Component c && Equals(c);
    }

    public bool Equals(Component other)
    {
        return Type == other.Type;
    }

    public static bool operator ==(Component a, Component b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Component a, Component b)
    {
        return !a.Equals(b);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type);
    }
}
