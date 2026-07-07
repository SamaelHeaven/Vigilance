using Vigilance.Math;

namespace Vigilance.Physics;

public record struct BodyDef
{
    public BodyDef()
    {
        SleepThreshold = 0.05f * World.PixelsPerMeter;
        GravityScale = 1f;
        EnableSleep = true;
        IsAwake = true;
        IsEnabled = true;
    }

    public BodyType Type { get; set; }
    public Vector2 Position { get; set; }
    public float Rotation { get; set; }
    public Vector2 LinearVelocity { get; set; }
    public float AngularVelocity { get; set; }
    public float LinearDamping { get; set; }
    public float AngularDamping { get; set; }
    public float GravityScale { get; set; }
    public float SleepThreshold { get; set; }
    public bool LockLinearX { get; set; }
    public bool LockLinearY { get; set; }
    public bool LockAngularZ { get; set; }
    public bool EnableSleep { get; set; }
    public bool IsAwake { get; set; }
    public bool IsBullet { get; set; }
    public bool IsEnabled { get; set; }
    public bool AllowFastRotation { get; set; }
}
