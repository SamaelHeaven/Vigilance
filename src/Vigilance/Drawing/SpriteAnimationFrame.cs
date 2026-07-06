using System.Runtime.InteropServices;
using Vigilance.Core;
using Vigilance.Math;
using Vigilance.UI;

namespace Vigilance.Drawing;

[StructLayout(LayoutKind.Sequential)]
public record struct SpriteAnimationFrame : IAnimationFrame
{
    public Texture? Texture { get; set; }
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

    public void Apply(Entity entity)
    {
        {
            if (entity.TryGet(out SpriteInstance sprite))
            {
                var newSprite = sprite;
                Apply(ref newSprite);
                if (sprite != newSprite)
                    entity.Set(newSprite);
            }
        }
        {
            if (entity.TryGet(out BatchedSprite sprite))
            {
                var newSprite = sprite;
                Apply(ref newSprite);
                if (sprite != newSprite)
                    entity.Set(newSprite);
            }
        }
        {
            if (entity.TryGet(out Sprite sprite))
                Apply(sprite);
        }
        {
            if (entity.TryGet(out UISprite sprite))
                Apply(sprite);
        }
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
