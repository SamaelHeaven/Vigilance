using Vigilance.Math;

namespace Vigilance.Core;

internal readonly record struct Position(Vector2 Value)
{
    public Position()
        : this(Vector2.Zero) { }
}
