namespace Vigilance.Physics;

public record struct ShapeDef
{
    public ShapeDef()
    {
        Friction = 0.6f;
        Density = 1f;
        Filter = new ShapeFilter();
        EnableContactEvents = true;
        EnableSensorEvents = true;
    }

    public float Friction { get; set; }
    public float Restitution { get; set; }
    public float RollingResistance { get; set; }
    public float TangentSpeed { get; set; }
    public float Density { get; set; }
    public bool IsSensor { get; set; }
    public ShapeFilter Filter { get; set; }
    public object? Data { get; set; }
    public bool EnableContactEvents { get; set; }
    public bool EnableSensorEvents { get; set; }
    public bool EnableHitEvents { get; set; }
}
