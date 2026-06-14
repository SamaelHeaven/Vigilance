using Vigilance.Math;

namespace Vigilance.UI;

public record struct Dimensions
{
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

    public Unit X { get; set; }
    public Unit Y { get; set; }

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

    public static Dimensions operator -(in Dimensions dimensions)
    {
        return new Dimensions(-dimensions.X, -dimensions.Y);
    }

    public static Dimensions operator +(in Dimensions dimensions, float value)
    {
        return new Dimensions(dimensions.X + value, dimensions.Y + value);
    }

    public static Dimensions operator -(in Dimensions dimensions, float value)
    {
        return new Dimensions(dimensions.X - value, dimensions.Y - value);
    }

    public static Dimensions operator +(in Dimensions dimensions, Unit value)
    {
        return new Dimensions(dimensions.X + value, dimensions.Y + value);
    }

    public static Dimensions operator -(in Dimensions dimensions, Unit value)
    {
        return new Dimensions(dimensions.X - value, dimensions.Y - value);
    }

    public static Dimensions operator +(in Dimensions dimensions, in Dimensions value)
    {
        return new Dimensions(dimensions.X + value.X, dimensions.Y + value.Y);
    }

    public static Dimensions operator -(in Dimensions dimensions, in Dimensions value)
    {
        return new Dimensions(dimensions.X - value.X, dimensions.Y - value.Y);
    }

    public readonly Vector2 Calculate(Vector2 layoutSize)
    {
        return new Vector2(X.Calculate(layoutSize.X), Y.Calculate(layoutSize.Y));
    }
}
