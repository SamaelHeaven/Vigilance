using Vigilance.Math;

namespace Vigilance.Core;

public readonly record struct Anchor
{
    private readonly Func<Vector2>? _positionFunc;
    private readonly Func<Vector2>? _scaleFunc;

    public Anchor(Vector2 origin, Func<Vector2> positionFunc, Func<Vector2> scaleFunc)
    {
        _positionFunc = positionFunc;
        _scaleFunc = scaleFunc;
        Origin = origin;
    }

    public Anchor(Vector2 origin, Func<Vector2> positionFunc, Vector2? scale = null)
    {
        _positionFunc = positionFunc;
        Scale = scale;
        Origin = origin;
    }

    public Anchor(Vector2 origin, Vector2 position, Func<Vector2> scaleFunc)
    {
        _scaleFunc = scaleFunc;
        Position = position;
        Origin = origin;
    }

    public Anchor(Vector2 origin, Vector2 position, Vector2? scale = null)
    {
        Scale = scale;
        Position = position;
        Origin = origin;
    }

    public Vector2 Origin { get; }
    public Vector2 Position => _positionFunc?.SafeInvoke() ?? field;
    public Vector2? Scale => _scaleFunc?.SafeInvoke() ?? field;

    public static Anchor TopLeft(Func<Vector2> positionFunc, Func<Vector2> scaleFunc)
    {
        return new Anchor(new Vector2(-1), positionFunc, scaleFunc);
    }

    public static Anchor TopLeft(Func<Vector2> positionFunc, Vector2? scale = null)
    {
        return new Anchor(new Vector2(-1), positionFunc, scale);
    }

    public static Anchor TopLeft(Vector2 position, Func<Vector2> scaleFunc)
    {
        return new Anchor(new Vector2(-1), position, scaleFunc);
    }

    public static Anchor TopLeft(Vector2 position, Vector2? scale = null)
    {
        return new Anchor(new Vector2(-1), position, scale);
    }

    public static Anchor Top(Func<Vector2> positionFunc, Func<Vector2> scaleFunc)
    {
        return new Anchor(new Vector2(0, -1), positionFunc, scaleFunc);
    }

    public static Anchor Top(Func<Vector2> positionFunc, Vector2? scale = null)
    {
        return new Anchor(new Vector2(0, -1), positionFunc, scale);
    }

    public static Anchor Top(Vector2 position, Func<Vector2> scaleFunc)
    {
        return new Anchor(new Vector2(0, -1), position, scaleFunc);
    }

    public static Anchor Top(Vector2 position, Vector2? scale = null)
    {
        return new Anchor(new Vector2(0, -1), position, scale);
    }

    public static Anchor TopRight(Func<Vector2> positionFunc, Func<Vector2> scaleFunc)
    {
        return new Anchor(new Vector2(1, -1), positionFunc, scaleFunc);
    }

    public static Anchor TopRight(Func<Vector2> positionFunc, Vector2? scale = null)
    {
        return new Anchor(new Vector2(1, -1), positionFunc, scale);
    }

    public static Anchor TopRight(Vector2 position, Func<Vector2> scaleFunc)
    {
        return new Anchor(new Vector2(1, -1), position, scaleFunc);
    }

    public static Anchor TopRight(Vector2 position, Vector2? scale = null)
    {
        return new Anchor(new Vector2(1, -1), position, scale);
    }

    public static Anchor Right(Func<Vector2> positionFunc, Func<Vector2> scaleFunc)
    {
        return new Anchor(new Vector2(1, 0), positionFunc, scaleFunc);
    }

    public static Anchor Right(Func<Vector2> positionFunc, Vector2? scale = null)
    {
        return new Anchor(new Vector2(1, 0), positionFunc, scale);
    }

    public static Anchor Right(Vector2 position, Func<Vector2> scaleFunc)
    {
        return new Anchor(new Vector2(1, 0), position, scaleFunc);
    }

    public static Anchor Right(Vector2 position, Vector2? scale = null)
    {
        return new Anchor(new Vector2(1, 0), position, scale);
    }

    public static Anchor BottomLeft(Func<Vector2> positionFunc, Func<Vector2> scaleFunc)
    {
        return new Anchor(new Vector2(-1, 1), positionFunc, scaleFunc);
    }

    public static Anchor BottomLeft(Func<Vector2> positionFunc, Vector2? scale = null)
    {
        return new Anchor(new Vector2(-1, 1), positionFunc, scale);
    }

    public static Anchor BottomLeft(Vector2 position, Func<Vector2> scaleFunc)
    {
        return new Anchor(new Vector2(-1, 1), position, scaleFunc);
    }

    public static Anchor BottomLeft(Vector2 position, Vector2? scale = null)
    {
        return new Anchor(new Vector2(-1, 1), position, scale);
    }

    public static Anchor Bottom(Func<Vector2> positionFunc, Func<Vector2> scaleFunc)
    {
        return new Anchor(new Vector2(0, 1), positionFunc, scaleFunc);
    }

    public static Anchor Bottom(Func<Vector2> positionFunc, Vector2? scale = null)
    {
        return new Anchor(new Vector2(0, 1), positionFunc, scale);
    }

    public static Anchor Bottom(Vector2 position, Func<Vector2> scaleFunc)
    {
        return new Anchor(new Vector2(0, 1), position, scaleFunc);
    }

    public static Anchor Bottom(Vector2 position, Vector2? scale = null)
    {
        return new Anchor(new Vector2(0, 1), position, scale);
    }

    public static Anchor BottomRight(Func<Vector2> positionFunc, Func<Vector2> scaleFunc)
    {
        return new Anchor(new Vector2(1), positionFunc, scaleFunc);
    }

    public static Anchor BottomRight(Func<Vector2> positionFunc, Vector2? scale = null)
    {
        return new Anchor(new Vector2(1), positionFunc, scale);
    }

    public static Anchor BottomRight(Vector2 position, Func<Vector2> scaleFunc)
    {
        return new Anchor(new Vector2(1), position, scaleFunc);
    }

    public static Anchor BottomRight(Vector2 position, Vector2? scale = null)
    {
        return new Anchor(new Vector2(1), position, scale);
    }

    public static Anchor Left(Func<Vector2> positionFunc, Func<Vector2> scaleFunc)
    {
        return new Anchor(new Vector2(-1, 0), positionFunc, scaleFunc);
    }

    public static Anchor Left(Func<Vector2> positionFunc, Vector2? scale = null)
    {
        return new Anchor(new Vector2(-1, 0), positionFunc, scale);
    }

    public static Anchor Left(Vector2 position, Func<Vector2> scaleFunc)
    {
        return new Anchor(new Vector2(-1, 0), position, scaleFunc);
    }

    public static Anchor Left(Vector2 position, Vector2? scale = null)
    {
        return new Anchor(new Vector2(-1, 0), position, scale);
    }
}
