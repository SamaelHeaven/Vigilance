using FlexLayoutSharp;

namespace Vigilance.UI;

public struct Unit
{
    public UnitType Type { get; set; }
    public float Value { get; set; }

    public static Unit Auto => new() { Type = UnitType.Auto };
    public static Unit Zero => new() { Type = UnitType.Fixed };
    public static Unit NaN => new() { Type = UnitType.Fixed, Value = float.NaN };
    public static Unit Full => new() { Type = UnitType.Percent, Value = 100 };
    public static Unit Half => new() { Type = UnitType.Percent, Value = 50 };
    public static Unit Undefined => new() { Type = UnitType.Undefined };

    public static Unit Fixed(float value)
    {
        return new Unit { Type = UnitType.Fixed, Value = value };
    }

    public static Unit Percent(float value)
    {
        return new Unit { Type = UnitType.Percent, Value = value };
    }

    public static implicit operator Unit(float value)
    {
        return Fixed(value);
    }

    public float Calculate(float size)
    {
        return Type switch
        {
            UnitType.Fixed => Value,
            UnitType.Percent => Value * size / 100f,
            _ => 0,
        };
    }

    internal static Unit FromValue(Value value)
    {
        var type = value.unit switch
        {
            FlexLayoutSharp.Unit.Auto => UnitType.Auto,
            FlexLayoutSharp.Unit.Percent => UnitType.Percent,
            FlexLayoutSharp.Unit.Point => UnitType.Fixed,
            _ => UnitType.Undefined,
        };
        return new Unit { Type = type, Value = value.value };
    }

    internal static void SetUnit(Unit value, Action setAuto, Action<float> setFixed, Action<float> setPercent)
    {
        switch (value.Type)
        {
            case UnitType.Undefined:
            case UnitType.Auto:
                setAuto.Invoke();
                break;
            case UnitType.Fixed:
                setFixed.Invoke(value.Value);
                break;
            case UnitType.Percent:
                setPercent.Invoke(value.Value);
                break;
        }
    }

    internal static void SetUnit(Unit value, Action<float> setFixed, Action<float> setPercent)
    {
        switch (value.Type)
        {
            case UnitType.Undefined:
            case UnitType.Auto:
                setFixed.Invoke(float.NaN);
                break;
            case UnitType.Fixed:
                setFixed.Invoke(value.Value);
                break;
            case UnitType.Percent:
                setPercent.Invoke(value.Value);
                break;
        }
    }

    internal static void SetUnit(
        Unit value,
        Edge edge,
        Action<Edge> setAuto,
        Action<Edge, float> setFixed,
        Action<Edge, float> setPercent
    )
    {
        switch (value.Type)
        {
            case UnitType.Undefined:
                setFixed.Invoke(edge, float.NaN);
                break;
            case UnitType.Auto:
                setAuto.Invoke(edge);
                break;
            case UnitType.Fixed:
                setFixed.Invoke(edge, value.Value);
                break;
            case UnitType.Percent:
                setPercent.Invoke(edge, value.Value);
                break;
        }
    }

    internal static void SetUnit(Unit value, Edge edge, Action<Edge, float> setFixed, Action<Edge, float> setPercent)
    {
        switch (value.Type)
        {
            case UnitType.Undefined:
            case UnitType.Auto:
                setFixed.Invoke(edge, float.NaN);
                break;
            case UnitType.Fixed:
                setFixed.Invoke(edge, value.Value);
                break;
            case UnitType.Percent:
                setPercent.Invoke(edge, value.Value);
                break;
        }
    }
}

public enum UnitType
{
    Undefined,
    Auto,
    Fixed,
    Percent,
}
