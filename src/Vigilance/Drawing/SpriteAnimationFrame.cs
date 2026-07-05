using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class SpriteAnimationFrame : ISpriteAnimationFrame, IFullCloneable
{
    public Texture? Texture { get; set; } = null;
    public bool? FlipX { get; set; } = null;
    public bool? FlipY { get; set; } = null;
    public Wrapper<Box?>? Source { get; set; } = null;
    public Color? Tint { get; set; } = null;
    public Wrapper<NPatchInfo?>? NPatchInfo { get; set; } = null;
    public TextureFilter? TextureFilter { get; set; } = null;
    public Vector2? Position { get; set; } = null;
    public Vector2? Scale { get; set; } = null;
    public float? Rotation { get; set; } = null;
    public Vector2? PivotPoint { get; set; } = null;
    public Wrapper<BlendMode?>? BlendMode { get; set; } = null;
    public Wrapper<Shader?>? Shader { get; set; } = null;
    public Wrapper<bool?>? Culling { get; set; } = null;
    public Wrapper<Action<Transform, Sprite, Graphics>?>? OnBeginDrawing { get; set; } = null;
    public Wrapper<Action<Transform, Sprite, Graphics>?>? OnEndDrawing { get; set; } = null;

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

    public TimeSpan Delay { get; set; } = TimeSpan.Zero;

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
        if (TextureFilter.HasValue)
            sprite.TextureFilter = TextureFilter.Value;
        if (Position.HasValue)
            sprite.Position = Position.Value;
        if (Scale.HasValue)
            sprite.Scale = Scale.Value;
        if (Rotation.HasValue)
            sprite.Rotation = Rotation.Value;
        if (PivotPoint.HasValue)
            sprite.PivotPoint = PivotPoint.Value;
        if (BlendMode.HasValue)
            sprite.BlendMode = BlendMode.Value;
        if (Shader.HasValue)
            sprite.Shader = Shader;
        if (Culling.HasValue)
            sprite.Culling = Culling.Value;
        if (OnBeginDrawing.HasValue)
            sprite.OnBeginDrawing = OnBeginDrawing.Value;
        if (OnEndDrawing.HasValue)
            sprite.OnEndDrawing = OnEndDrawing.Value;
    }

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
    }
}
