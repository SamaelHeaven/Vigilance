using System.Runtime.InteropServices;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

[StructLayout(LayoutKind.Sequential)]
public record struct BatchedSpriteAnimationFrame : IAnimationFrame
{
    public Wrapper<Box?>? Source { get; set; }
    public TimeSpan Delay { get; set; }
    public Vector2? Position { get; set; }
    public Vector2? Scale { get; set; }
    public Vector2? PivotPoint { get; set; }
    public Color? Tint { get; set; }
    public float? Rotation { get; set; }
    public bool? FlipX { get; set; }
    public bool? FlipY { get; set; }

    public Transform Transform
    {
        set
        {
            Position = value.Position;
            Scale = value.Scale;
            Rotation = value.Rotation;
            PivotPoint = value.PivotPoint;
        }
    }

    public readonly void Apply(Entity entity)
    {
        if (!entity.TryGet(out BatchedSprite sprite))
            return;
        var newSprite = sprite;
        Apply(ref newSprite);
        if (sprite != newSprite)
            entity.Set(newSprite);
    }

    public readonly void Apply(ref BatchedSprite sprite)
    {
        var instance = sprite.Instance;
        Apply(ref instance);
        sprite.Instance = instance;
    }

    public readonly void Apply(ref SpriteInstance sprite)
    {
        if (FlipX.HasValue)
            sprite.FlipX = FlipX.Value;
        if (FlipY.HasValue)
            sprite.FlipY = FlipY.Value;
        if (Source.HasValue)
            sprite.Source = Source;
        if (Tint.HasValue)
            sprite.Tint = Tint.Value;
        if (Position.HasValue)
            sprite.Position = Position.Value;
        if (Scale.HasValue)
            sprite.Scale = Scale.Value;
        if (Rotation.HasValue)
            sprite.Rotation = Rotation.Value;
        if (PivotPoint.HasValue)
            sprite.PivotPoint = PivotPoint.Value;
    }
}
