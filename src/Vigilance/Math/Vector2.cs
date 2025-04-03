namespace Vigilance.Math;

public struct Vector2
{
    public float X;
    public float Y;

    public static readonly Vector2 Zero = new(0);
    public static readonly Vector2 One = new(1);
    public static readonly Vector2 Up = new(0, -1);
    public static readonly Vector2 Down = new(0, 1);
    public static readonly Vector2 Left = new(-1, 0);
    public static readonly Vector2 Right = new(1, 0);

    public Vector2(float? v1 = null, float? v2 = null)
    {
        X = v1 ?? 0;
        Y = v2 ?? X;
    }

    public static implicit operator System.Numerics.Vector2(Vector2 v)
    {
        return new System.Numerics.Vector2(v.X, v.Y);
    }

    public static implicit operator Vector2(System.Numerics.Vector2 v)
    {
        return new Vector2(v.X, v.Y);
    }

    public static implicit operator (float, float)(Vector2 v)
    {
        return (v.X, v.Y);
    }

    public static implicit operator Vector2((float, float) v)
    {
        return new Vector2(v.Item1, v.Item2);
    }

    public static implicit operator Vector2(float v)
    {
        return new Vector2(v);
    }

    public (float, float) ToTuple()
    {
        return this;
    }

    public override string ToString()
    {
        return $"{{ X: {X}, Y: {Y} }}";
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector2 v && X.Equals(v.X) && Y.Equals(v.Y);
    }

    public static bool operator ==(Vector2 a, Vector2 b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Vector2 a, Vector2 b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static Vector2 operator -(Vector2 v)
    {
        return new Vector2(-v.X, -v.Y);
    }

    public static Vector2 operator +(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X + b.X, a.Y + b.Y);
    }

    public static Vector2 operator -(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X - b.X, a.Y - b.Y);
    }

    public static Vector2 operator *(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X * b.X, a.Y * b.Y);
    }

    public static Vector2 operator /(Vector2 a, Vector2 b)
    {
        return new Vector2(b.X == 0 ? 0 : a.X / b.X, b.Y == 0 ? 0 : a.Y / b.Y);
    }

    public static Vector2 operator +(Vector2 v, float f)
    {
        return new Vector2(v.X + f, v.Y + f);
    }

    public static Vector2 operator -(Vector2 v, float f)
    {
        return new Vector2(v.X - f, v.Y - f);
    }

    public static Vector2 operator *(Vector2 v, float f)
    {
        return new Vector2(v.X * f, v.Y * f);
    }

    public static Vector2 operator /(Vector2 v, float f)
    {
        return f == 0 ? Zero : new Vector2(v.X / f, v.Y / f);
    }

    public float DistanceTo(Vector2 v)
    {
        var d = this - v;
        return MathF.Sqrt(d.X * d.X + d.Y * d.Y);
    }

    public float Dot(Vector2 v)
    {
        return X * v.X + Y * v.Y;
    }

    public float Cross(Vector2 v)
    {
        return X * v.X - Y * v.Y;
    }

    public Vector2 Clamp(Vector2 min, Vector2 max)
    {
        return ClampX(min.X, max.X).ClampY(min.Y, max.Y);
    }

    public Vector2 Clamp(float min, float max)
    {
        return ClampX(min, max).ClampY(min, max);
    }

    public Vector2 ClampX(float min, float max)
    {
        return new Vector2(System.Math.Clamp(X, min, max), Y);
    }

    public Vector2 ClampY(float min, float max)
    {
        return new Vector2(X, System.Math.Clamp(Y, min, max));
    }

    public float Length()
    {
        return MathF.Sqrt(X * X + Y * Y);
    }

    public float AngleBetween(Vector2 v)
    {
        return Length() * v.Length() == 0 ? 0 : MathF.Acos(Dot(v) / (Length() * v.Length()));
    }

    public Vector2 Reflect(Vector2 normal)
    {
        var dot = Dot(normal);
        return new Vector2(X - 2 * dot * normal.X, Y - 2 * dot * normal.Y);
    }

    public Vector2 Rotate(float degrees, Vector2 origin)
    {
        var rad = degrees * (MathF.PI / 180);
        var cos = MathF.Cos(rad);
        var sin = MathF.Sin(rad);
        var translated = this - origin;
        return new Vector2(translated.X * cos - translated.Y * sin, translated.X * sin + translated.Y * cos) + origin;
    }

    public Vector2 Lerp(Vector2 end, float t)
    {
        t = System.Math.Clamp(t, 0f, 1f);
        return new Vector2(X + (end.X - X) * t, Y + (end.Y - Y) * t);
    }

    public Vector2 Slerp(Vector2 end, float t)
    {
        var angle = AngleBetween(end);
        return this * MathF.Cos(angle * t) + end * MathF.Sin(angle * t);
    }

    public Vector2 Round()
    {
        return new Vector2(MathF.Round(X), MathF.Round(Y));
    }

    public Vector2 Abs()
    {
        return new Vector2(MathF.Abs(X), MathF.Abs(Y));
    }

    public Vector2 Normalize()
    {
        return Length() == 0 ? Zero : this / Length();
    }

    public float ModifierX()
    {
        return X == 0 ? 0
            : X > 0 ? 1
            : -1;
    }

    public float ModifierY()
    {
        return Y == 0 ? 0
            : Y > 0 ? 1
            : -1;
    }

    public Vector2 Modifiers()
    {
        return new Vector2(ModifierX(), ModifierY());
    }
}
