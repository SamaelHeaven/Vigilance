using Vigilance.Math;

namespace Vigilance.Core;

internal readonly record struct Scale(Vector2 Value)
{
    public Scale()
        : this(Vector2.One) { }
}
