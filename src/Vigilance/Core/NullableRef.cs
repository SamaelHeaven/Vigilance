namespace Vigilance.Core;

public readonly ref struct NullableRef<T>
    where T : struct, allows ref struct
{
    private readonly T _value;

    public NullableRef(scoped in T value)
    {
        _value = value;
        HasValue = true;
    }

    public bool HasValue { get; }

    public T Value => !HasValue ? throw new NullReferenceException() : _value;

    public T GetValueOrDefault()
    {
        return _value;
    }

    public T GetValueOrDefault(scoped in T defaultValue)
    {
        return HasValue ? _value : defaultValue;
    }

    public static implicit operator NullableRef<T>(scoped in T value)
    {
        return new NullableRef<T>(value);
    }

    public static explicit operator T(NullableRef<T> value)
    {
        return value.Value;
    }
}
