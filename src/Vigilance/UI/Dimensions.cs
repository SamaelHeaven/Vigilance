using Vigilance.Math;

namespace Vigilance.UI;

public struct Dimensions
{
    public Unit X { get; set; } = Unit.Zero;
    public Unit Y { get; set; } = Unit.Zero;

    public Dimensions(Unit value)
    {
        X = value;
        Y = value;
    }

    public Dimensions(Unit x, Unit y)
    {
        X = x;
        Y = y;
    }

    public static implicit operator Dimensions(Vector2 value)
    {
        return new Dimensions(value.X, value.Y);
    }

    public static implicit operator Dimensions(Unit value)
    {
        return new Dimensions(value);
    }

    public static implicit operator Dimensions((Unit Width, Unit Height) value)
    {
        return new Dimensions(value.Width, value.Height);
    }

    public static implicit operator Dimensions(float value)
    {
        return new Dimensions(value);
    }

    public Vector2 Calculate(Vector2 layoutSize)
    {
        return new Vector2(X.Calculate(layoutSize.X), Y.Calculate(layoutSize.Y));
    }
}
