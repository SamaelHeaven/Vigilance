namespace Vigilance.Physics;

public readonly record struct RayCastHit
{
    public bool Hit { get; init; }
    public Shape Shape { get; init; }
    public Vector2 Point { get; init; }
    public Vector2 Normal { get; init; }
    public float Fraction { get; init; }
}
