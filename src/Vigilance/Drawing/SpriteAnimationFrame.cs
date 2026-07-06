using System.Runtime.InteropServices;
using Vigilance.Core;
using Vigilance.Math;
using Vigilance.UI;

namespace Vigilance.Drawing;

[StructLayout(LayoutKind.Sequential)]
public record struct SpriteAnimationFrame() : IAnimationFrame
{
    public Texture? Texture { get; set; } = null;
    public Wrapper<Box?>? Source { get; set; } = null;
    public TimeSpan Delay { get; set; } = TimeSpan.Zero;
    public Vector2? Position { get; set; } = null;
    public Vector2? Scale { get; set; } = null;
    public Vector2? PivotPoint { get; set; } = null;
    public Color? Tint { get; set; } = null;
    public float? Rotation { get; set; } = null;
    public bool? FlipX { get; set; } = null;
    public bool? FlipY { get; set; } = null;

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

    public void Apply(Entity entity)
    {
        if (entity.TryGet(out SpriteInstance spriteInstance))
        {
            var newSpriteInstance = spriteInstance;
            Apply(ref newSpriteInstance);
            if (spriteInstance != newSpriteInstance)
                entity.Set(newSpriteInstance);
        }

        if (entity.TryGet(out BatchedSprite batchedSprite))
        {
            var newBatchedSprite = batchedSprite;
            Apply(ref newBatchedSprite);
            if (batchedSprite != newBatchedSprite)
                entity.Set(newBatchedSprite);
        }

        if (entity.TryGet(out Sprite sprite))
            Apply(sprite);
        if (entity.TryGet(out UISprite uiSprite))
            Apply(uiSprite);
    }

    public void Apply(Sprite sprite)
    {
        if (Texture is not null)
            sprite.Texture = Texture;
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

    public void Apply(UISprite sprite)
    {
        if (Texture is not null)
            sprite.Texture = Texture;
        if (FlipX.HasValue)
            sprite.FlipX = FlipX.Value;
        if (FlipY.HasValue)
            sprite.FlipY = FlipY.Value;
        if (Source.HasValue)
            sprite.Source = Source;
        if (Tint.HasValue)
            sprite.Tint = Tint.Value;
        if (Position.HasValue)
            sprite.Translate = Position.Value;
        if (Scale.HasValue)
            sprite.Scale = Scale.Value;
        if (Rotation.HasValue)
            sprite.Rotation = Rotation.Value;
        if (PivotPoint.HasValue)
            sprite.PivotPoint = PivotPoint.Value;
    }

    public void Apply(ref BatchedSprite sprite)
    {
        var instance = sprite.Instance;
        Apply(ref instance);
        sprite.Instance = instance;
    }

    public void Apply(ref SpriteInstance sprite)
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
