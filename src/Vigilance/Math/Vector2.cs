using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Box2D.NET;

namespace Vigilance.Math;

[StructLayout(LayoutKind.Sequential)]
public record struct Vector2
    : IAdditionOperators<Vector2, Vector2, Vector2>,
        IAdditionOperators<Vector2, float, Vector2>,
        ISubtractionOperators<Vector2, Vector2, Vector2>,
        ISubtractionOperators<Vector2, float, Vector2>,
        IMultiplyOperators<Vector2, Vector2, Vector2>,
        IMultiplyOperators<Vector2, float, Vector2>,
        IDivisionOperators<Vector2, Vector2, Vector2>,
        IDivisionOperators<Vector2, float, Vector2>,
        IUnaryNegationOperators<Vector2, Vector2>,
        IUnaryPlusOperators<Vector2, Vector2>,
        IComparisonOperators<Vector2, Vector2, bool>
{
    public float X { get; set; }
    public float Y { get; set; }
    public static Vector2 Zero => default;
    public static Vector2 One => new(1);
    public static Vector2 Up => new(0, -1);
    public static Vector2 Down => new(0, 1);
    public static Vector2 Left => new(-1, 0);
    public static Vector2 Right => new(1, 0);
    public static Vector2 NaN => new(float.NaN);
    public static Vector2 PositiveInfinity => new(float.PositiveInfinity);
    public static Vector2 NegativeInfinity => new(float.NegativeInfinity);

    public Vector2(float value)
    {
        X = value;
        Y = value;
    }

    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    internal Vector2(B2Vec2 vec2)
    {
        X = vec2.X;
        Y = vec2.Y;
    }

    internal readonly B2Vec2 B2Vec2 => new(X, Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator System.Numerics.Vector2(Vector2 v)
    {
        return new System.Numerics.Vector2(v.X, v.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector2(System.Numerics.Vector2 v)
    {
        return new Vector2(v.X, v.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator (float X, float Y)(Vector2 v)
    {
        return (v.X, v.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector2((float X, float Y) v)
    {
        return new Vector2(v.X, v.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector2(float v)
    {
        return new Vector2(v);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Deconstruct(out float x, out float y)
    {
        x = X;
        y = Y;
    }

    public override readonly string ToString()
    {
        return $"<{X}, {Y}>";
    }

    public readonly string ToString(int digits)
    {
        digits = digits.Clamp(0, 8);
        var format = digits switch
        {
            0 => "F0",
            1 => "F1",
            2 => "F2",
            3 => "F3",
            4 => "F4",
            5 => "F5",
            6 => "F6",
            7 => "F7",
            _ => "F8",
        };
        return $"<{X.ToString(format)}, {Y.ToString(format)}>";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator -(Vector2 v)
    {
        return new Vector2(-v.X, -v.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator +(Vector2 value)
    {
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator +(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X + b.X, a.Y + b.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator -(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X - b.X, a.Y - b.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X * b.X, a.Y * b.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator /(Vector2 a, Vector2 b)
    {
        return new Vector2(b.X == 0 ? 0 : a.X / b.X, b.Y == 0 ? 0 : a.Y / b.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator +(Vector2 v, float f)
    {
        return new Vector2(v.X + f, v.Y + f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator -(Vector2 v, float f)
    {
        return new Vector2(v.X - f, v.Y - f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(Vector2 v, float f)
    {
        return new Vector2(v.X * f, v.Y * f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator /(Vector2 v, float f)
    {
        return f == 0 ? Zero : new Vector2(v.X / f, v.Y / f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Vector2 a, Vector2 b)
    {
        return a.X < b.X && a.Y < b.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Vector2 a, Vector2 b)
    {
        return a.X <= b.X && a.Y <= b.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Vector2 a, Vector2 b)
    {
        return a.X > b.X && a.Y > b.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Vector2 a, Vector2 b)
    {
        return a.X >= b.X && a.Y >= b.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Vector2 v, float f)
    {
        return v.X < f && v.Y < f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Vector2 v, float f)
    {
        return v.X <= f && v.Y <= f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Vector2 v, float f)
    {
        return v.X > f && v.Y > f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Vector2 v, float f)
    {
        return v.X >= f && v.Y >= f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Lerp(Vector2 start, Vector2 end, float t)
    {
        return new Vector2(float.Lerp(start.X, end.X, t), float.Lerp(start.Y, end.Y, t));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Slerp(Vector2 start, Vector2 end, float t)
    {
        var angle = start.AngleBetween(end);
        return start * MathF.Cos(angle * t) + end * MathF.Sin(angle * t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Transform(in Matrix3x2 matrix)
    {
        return System.Numerics.Vector2.Transform(this, matrix);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Transform(in Matrix4x4 matrix)
    {
        return System.Numerics.Vector2.Transform(this, matrix);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Transform(in Quaternion quaternion)
    {
        return System.Numerics.Vector2.Transform(this, quaternion);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float DistanceTo(Vector2 v)
    {
        var d = this - v;
        return MathF.Sqrt(d.X * d.X + d.Y * d.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Dot(Vector2 v)
    {
        return X * v.X + Y * v.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Cross(Vector2 v)
    {
        return X * v.X - Y * v.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Min()
    {
        return X.Min(Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Min(Vector2 min)
    {
        return new Vector2(X.Min(min.X), Y.Min(min.Y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 MinX(float min)
    {
        return new Vector2(X.Min(min), Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 MinY(float min)
    {
        return new Vector2(X, Y.Min(min));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Max()
    {
        return X.Max(Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Max(Vector2 max)
    {
        return new Vector2(X.Max(max.X), Y.Max(max.Y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 MaxX(float max)
    {
        return new Vector2(X.Max(max), Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 MaxY(float max)
    {
        return new Vector2(X, Y.Max(max));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Clamp(Vector2 min, Vector2 max)
    {
        return ClampX(min.X, max.X).ClampY(min.Y, max.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Clamp(float min, float max)
    {
        return ClampX(min, max).ClampY(min, max);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 ClampX(float min, float max)
    {
        return new Vector2(X.Clamp(min, max), Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 ClampY(float min, float max)
    {
        return new Vector2(X, Y.Clamp(min, max));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float Length()
    {
        return MathF.Sqrt(X * X + Y * Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float LengthSquared()
    {
        return X * X + Y * Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float AngleBetween(Vector2 v)
    {
        return Length() * v.Length() == 0 ? 0 : MathF.Acos(Dot(v) / (Length() * v.Length()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Reflect(Vector2 normal)
    {
        var dot = Dot(normal);
        return new Vector2(X - 2 * dot * normal.X, Y - 2 * dot * normal.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Rotate(float degrees, Vector2 origin)
    {
        var rad = degrees.DegToRad();
        var cos = MathF.Cos(rad);
        var sin = MathF.Sin(rad);
        var translated = this - origin;
        return new Vector2(translated.X * cos - translated.Y * sin, translated.X * sin + translated.Y * cos) + origin;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Round()
    {
        return new Vector2(X.Round(), Y.Round());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Round(int digits)
    {
        return new Vector2(X.Round(digits), Y.Round(digits));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Round(MidpointRounding mode)
    {
        return new Vector2(X.Round(mode), Y.Round(mode));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Round(int digits, MidpointRounding mode)
    {
        return new Vector2(X.Round(digits, mode), Y.Round(digits, mode));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Floor()
    {
        return new Vector2(X.Floor(), Y.Floor());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Ceil()
    {
        return new Vector2(X.Ceil(), Y.Ceil());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Abs()
    {
        return new Vector2(X.Abs(), Y.Abs());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Normalize()
    {
        return Length() == 0 ? Zero : this / Length();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector2 Signs()
    {
        return new Vector2(X.Sign(), Y.Sign());
    }
}
