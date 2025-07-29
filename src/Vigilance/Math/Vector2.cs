using System.Numerics;
using System.Runtime.InteropServices;

namespace Vigilance.Math;

[StructLayout(LayoutKind.Sequential)]
public struct Vector2 : IEquatable<Vector2>
{
    public float X { get; set; }
    public float Y { get; set; }

    public static Vector2 Zero { get; } = new(0);
    public static Vector2 One { get; } = new(1);
    public static Vector2 Up { get; } = new(0, -1);
    public static Vector2 Down { get; } = new(0, 1);
    public static Vector2 Left { get; } = new(-1, 0);
    public static Vector2 Right { get; } = new(1, 0);

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

    public static implicit operator (float X, float Y)(Vector2 v)
    {
        return (v.X, v.Y);
    }

    public static implicit operator Vector2((float X, float Y) v)
    {
        return new Vector2(v.X, v.Y);
    }

    public static implicit operator Vector2(float v)
    {
        return new Vector2(v);
    }

    public void Deconstruct(out float x, out float y)
    {
        x = X;
        y = Y;
    }

    public override string ToString()
    {
        return $"<{X}, {Y}>";
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector2 v && Equals(v);
    }

    public bool Equals(Vector2 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
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

    public readonly Vector2 Transform(Matrix3x2 matrix)
    {
        return System.Numerics.Vector2.Transform(this, matrix);
    }

    public readonly Vector2 Transform(Matrix4x4 matrix)
    {
        return System.Numerics.Vector2.Transform(this, matrix);
    }

    public readonly Vector2 Transform(Quaternion quaternion)
    {
        return System.Numerics.Vector2.Transform(this, quaternion);
    }

    public readonly float DistanceTo(Vector2 v)
    {
        var d = this - v;
        return MathF.Sqrt(d.X * d.X + d.Y * d.Y);
    }

    public readonly float Dot(Vector2 v)
    {
        return X * v.X + Y * v.Y;
    }

    public readonly float Cross(Vector2 v)
    {
        return X * v.X - Y * v.Y;
    }

    public readonly Vector2 Min(Vector2 min)
    {
        return new Vector2(X.Min(min.X), Y.Min(min.Y));
    }

    public readonly Vector2 MinX(float min)
    {
        return new Vector2(X.Min(min), Y);
    }

    public readonly Vector2 MinY(float min)
    {
        return new Vector2(X, Y.Min(min));
    }

    public readonly Vector2 Max(Vector2 max)
    {
        return new Vector2(X.Max(max.X), Y.Max(max.Y));
    }

    public readonly Vector2 MaxX(float max)
    {
        return new Vector2(X.Max(max), Y);
    }

    public readonly Vector2 MaxY(float max)
    {
        return new Vector2(X, Y.Max(max));
    }

    public readonly Vector2 Clamp(Vector2 min, Vector2 max)
    {
        return ClampX(min.X, max.X).ClampY(min.Y, max.Y);
    }

    public readonly Vector2 Clamp(float min, float max)
    {
        return ClampX(min, max).ClampY(min, max);
    }

    public readonly Vector2 ClampX(float min, float max)
    {
        return new Vector2(X.Clamp(min, max), Y);
    }

    public readonly Vector2 ClampY(float min, float max)
    {
        return new Vector2(X, Y.Clamp(min, max));
    }

    public readonly float Length()
    {
        return MathF.Sqrt(X * X + Y * Y);
    }

    public readonly float AngleBetween(Vector2 v)
    {
        return Length() * v.Length() == 0 ? 0 : MathF.Acos(Dot(v) / (Length() * v.Length()));
    }

    public readonly Vector2 Reflect(Vector2 normal)
    {
        var dot = Dot(normal);
        return new Vector2(X - 2 * dot * normal.X, Y - 2 * dot * normal.Y);
    }

    public readonly Vector2 Rotate(float degrees, Vector2 origin)
    {
        var rad = degrees.DegToRad();
        var cos = MathF.Cos(rad);
        var sin = MathF.Sin(rad);
        var translated = this - origin;
        return new Vector2(translated.X * cos - translated.Y * sin, translated.X * sin + translated.Y * cos) + origin;
    }

    public readonly Vector2 Lerp(Vector2 end, float t)
    {
        t = t.Clamp(0f, 1f);
        return new Vector2(X + (end.X - X) * t, Y + (end.Y - Y) * t);
    }

    public readonly Vector2 Slerp(Vector2 end, float t)
    {
        var angle = AngleBetween(end);
        return this * MathF.Cos(angle * t) + end * MathF.Sin(angle * t);
    }

    public readonly Vector2 Round()
    {
        return new Vector2(X.Round(), Y.Round());
    }

    public readonly Vector2 Floor()
    {
        return new Vector2(X.Floor(), Y.Floor());
    }

    public readonly Vector2 Ceil()
    {
        return new Vector2(X.Ceil(), Y.Ceil());
    }

    public readonly Vector2 Abs()
    {
        return new Vector2(X.Abs(), Y.Abs());
    }

    public readonly Vector2 Normalize()
    {
        return Length() == 0 ? Zero : this / Length();
    }

    public readonly float ModifierX()
    {
        return X == 0 ? 0
            : X > 0 ? 1
            : -1;
    }

    public readonly float ModifierY()
    {
        return Y == 0 ? 0
            : Y > 0 ? 1
            : -1;
    }

    public readonly Vector2 Modifiers()
    {
        return new Vector2(ModifierX(), ModifierY());
    }
}
