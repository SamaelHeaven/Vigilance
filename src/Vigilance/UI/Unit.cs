using System.Runtime.CompilerServices;
using Vigilance.FlexLayout;

namespace Vigilance.UI;

public record struct Unit(UnitType Type, float Value = 0)
{
    public static Unit Auto => new(UnitType.Auto);
    public static Unit Zero => new(UnitType.Fixed);
    public static Unit NaN => new(UnitType.Fixed, float.NaN);
    public static Unit Full => new(UnitType.Percent, 100);
    public static Unit Half => new(UnitType.Percent, 50);
    public static Unit Undefined => new(UnitType.Undefined);

    public static Unit Fixed(float value)
    {
        return new Unit(UnitType.Fixed, value);
    }

    public static Unit Percent(float value)
    {
        return new Unit(UnitType.Percent, value);
    }

    public static implicit operator Unit(float value)
    {
        return Fixed(value);
    }

    public static Unit operator -(Unit unit)
    {
        return unit with { Value = -unit.Value };
    }

    public static Unit operator +(Unit unit, float value)
    {
        return new Unit(unit.Type == UnitType.Undefined ? UnitType.Fixed : unit.Type, value + unit.Value);
    }

    public static Unit operator -(Unit unit, float value)
    {
        return new Unit(unit.Type == UnitType.Undefined ? UnitType.Fixed : unit.Type, value - unit.Value);
    }

    public static Unit operator +(Unit unit, Unit value)
    {
        return new Unit(unit.Type == UnitType.Undefined ? value.Type : unit.Type, value.Value + unit.Value);
    }

    public static Unit operator -(Unit unit, Unit value)
    {
        return new Unit(unit.Type == UnitType.Undefined ? value.Type : unit.Type, value.Value - unit.Value);
    }

    public readonly float Calculate(float size, float defaultValue = 0)
    {
        return Type switch
        {
            UnitType.Fixed => Value,
            UnitType.Percent => Value * size / 100f,
            _ => defaultValue,
        };
    }

    internal static Unit FromValue(Value value)
    {
        var type = value.Unit switch
        {
            FlexLayout.Unit.Auto => UnitType.Auto,
            FlexLayout.Unit.Percent => UnitType.Percent,
            FlexLayout.Unit.Point => UnitType.Fixed,
            _ => UnitType.Undefined,
        };
        return new Unit(type, value.Number);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SetUnit(
        UINode node,
        Unit value,
        Action<UINode> setAuto,
        Action<UINode, float> setFixed,
        Action<UINode, float> setPercent
    )
    {
        switch (value.Type)
        {
            case UnitType.Undefined:
            case UnitType.Auto:
                setAuto.Invoke(node);
                break;
            case UnitType.Fixed:
                setFixed.Invoke(node, value.Value);
                break;
            case UnitType.Percent:
                setPercent.Invoke(node, value.Value);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SetUnit(
        UINode node,
        Unit value,
        Action<UINode, float> setFixed,
        Action<UINode, float> setPercent
    )
    {
        switch (value.Type)
        {
            case UnitType.Undefined:
            case UnitType.Auto:
                setFixed.Invoke(node, float.NaN);
                break;
            case UnitType.Fixed:
                setFixed.Invoke(node, value.Value);
                break;
            case UnitType.Percent:
                setPercent.Invoke(node, value.Value);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SetUnit(
        UINode node,
        Unit value,
        Gutter gutter,
        Action<UINode, Gutter, float> setFixed,
        Action<UINode, Gutter, float> setPercent
    )
    {
        switch (value.Type)
        {
            case UnitType.Undefined:
            case UnitType.Auto:
                setFixed.Invoke(node, gutter, float.NaN);
                break;
            case UnitType.Fixed:
                setFixed.Invoke(node, gutter, value.Value);
                break;
            case UnitType.Percent:
                setPercent.Invoke(node, gutter, value.Value);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SetUnit(
        UINode node,
        Unit value,
        Edge edge,
        Action<UINode, Edge> setAuto,
        Action<UINode, Edge, float> setFixed,
        Action<UINode, Edge, float> setPercent
    )
    {
        switch (value.Type)
        {
            case UnitType.Undefined:
                setFixed.Invoke(node, edge, float.NaN);
                break;
            case UnitType.Auto:
                setAuto.Invoke(node, edge);
                break;
            case UnitType.Fixed:
                setFixed.Invoke(node, edge, value.Value);
                break;
            case UnitType.Percent:
                setPercent.Invoke(node, edge, value.Value);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SetUnit(
        UINode node,
        Unit value,
        Edge edge,
        Action<UINode, Edge, float> setFixed,
        Action<UINode, Edge, float> setPercent
    )
    {
        switch (value.Type)
        {
            case UnitType.Undefined:
            case UnitType.Auto:
                setFixed.Invoke(node, edge, float.NaN);
                break;
            case UnitType.Fixed:
                setFixed.Invoke(node, edge, value.Value);
                break;
            case UnitType.Percent:
                setPercent.Invoke(node, edge, value.Value);
                break;
        }
    }
}

public enum UnitType : sbyte
{
    Undefined,
    Auto,
    Fixed,
    Percent,
}
