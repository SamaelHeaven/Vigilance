namespace Vigilance.Core;

public readonly struct ComponentEntry(Type type, object? data = null)
{
    public Type Type { get; } = type;
    public object? Data { get; } = data;

    public override bool Equals(object? obj)
    {
        return obj is ComponentEntry c && Equals(c);
    }

    public bool Equals(ComponentEntry other)
    {
        return Type == other.Type;
    }

    public static bool operator ==(ComponentEntry a, ComponentEntry b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(ComponentEntry a, ComponentEntry b)
    {
        return !a.Equals(b);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type);
    }
}
