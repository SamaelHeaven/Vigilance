using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class SpriteAnimationFrame
{
    public TimeSpan Delay { get; set; } = TimeSpan.Zero;
    public Texture? Texture { get; set; } = null;
    public bool? FlipX { get; set; } = null;
    public bool? FlipY { get; set; } = null;
    public Wrapper<Box?>? Source { get; set; } = null;
    public Color? Tint { get; set; } = null;
    public Wrapper<NPatchInfo?>? NPatchInfo { get; set; } = null;
    public Interpolation? Interpolation { get; set; } = null;
    public Vector2? Position { get; set; } = Vector2.Zero;
    public Vector2? Scale { get; set; } = Vector2.One;
    public float? Rotation { get; set; } = 0;
    public Vector2? PivotPoint { get; set; } = Vector2.Zero;
    public Wrapper<Action<Transform, Sprite, Graphics>?>? OnBeginDrawing { get; set; }
    public Wrapper<Action<Transform, Sprite, Graphics>?>? OnEndDrawing { get; set; }

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

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform)));
    }

    public void UpdateSprite(Sprite sprite)
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
        if (NPatchInfo.HasValue)
            sprite.NPatchInfo = NPatchInfo;
        if (Interpolation.HasValue)
            sprite.Interpolation = Interpolation.Value;
        if (Position.HasValue)
            sprite.Position = Position.Value;
        if (Scale.HasValue)
            sprite.Scale = Scale.Value;
        if (Rotation.HasValue)
            sprite.Rotation = Rotation.Value;
        if (PivotPoint.HasValue)
            sprite.PivotPoint = PivotPoint.Value;
        if (OnBeginDrawing.HasValue)
            sprite.OnBeginDrawing = OnBeginDrawing.Value;
        if (OnEndDrawing.HasValue)
            sprite.OnEndDrawing = OnEndDrawing.Value;
    }
}
