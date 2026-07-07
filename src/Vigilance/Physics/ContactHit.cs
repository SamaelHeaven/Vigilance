using Vigilance.Math;

namespace Vigilance.Physics;

public readonly record struct ContactHit
{
    public Shape ShapeA { get; init; }
    public Shape ShapeB { get; init; }
    public Vector2 Point { get; init; }
    public Vector2 Normal { get; init; }
    public float ApproachSpeed { get; init; }
}
