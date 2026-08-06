namespace Vigilance.Drawing;

[ValueWrapper(typeof(Drawable<ValueSprite>), "Drawable")]
public partial struct ValueSprite : IDrawable
{
    public ValueSprite(Texture texture)
        : this()
    {
        Texture = texture;
        Scale = texture.Size;
    }

    public Texture Texture { get; set; } = Drawing.DefaultTexture;
    public bool FlipX { get; set; } = false;
    public bool FlipY { get; set; } = false;
    public Box? Source { get; set; } = null;
    public Color Tint { get; set; } = Color.White;
    public NPatchInfo? NPatchInfo { get; set; } = null;
    public TextureFilter TextureFilter { get; set; } = Drawing.DefaultTextureFilter;
    public TextureWrap TextureWrap { get; set; } = Drawing.DefaultTextureWrap;

    public override readonly string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
    }

    public readonly void Draw(Transform transform, Graphics graphics)
    {
        graphics.DrawSprite(transform, this);
    }
}

[ValueWrapper(typeof(ValueSprite))]
public sealed partial class Sprite : IDrawable, IFullCloneable
{
    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
    }
}

public static class SpriteExtensions
{
    extension(Graphics graphics)
    {
        public void DrawSprite(in ValueSprite sprite)
        {
            graphics.DrawSprite(new Transform(), sprite);
        }

        public void DrawSprite(float x, float y, float width, float height, in ValueSprite sprite)
        {
            graphics.DrawSprite(new Vector2(x, y), new Vector2(width, height), sprite);
        }

        public void DrawSprite(Vector2 position, Vector2 size, in ValueSprite sprite)
        {
            graphics.DrawSprite(new Transform(position + size * 0.5f, size), sprite);
        }

        public void DrawSprite(in Box box, in ValueSprite sprite)
        {
            graphics.DrawSprite(box.Position, box.Size, sprite);
        }

        public void DrawSprite(Transform transform, in ValueSprite sprite)
        {
            using var _ = Drawable<ValueSprite>.EnterDrawing(ref transform, sprite.Drawable, sprite, graphics);
            var camera = sprite.Camera.Get();
            var texture = sprite.Texture;
            var textureFilter = sprite.TextureFilter;
            var textureWrap = sprite.TextureWrap;
            var tint = sprite.Tint;
            var nPatchInfo = sprite.NPatchInfo;
            var flipX = sprite.FlipX;
            var flipY = sprite.FlipY;
            var position = transform.Position;
            var scale = transform.Scale.Abs();
            var source = sprite.Source ?? new Box(Vector2.Zero, texture.Size);
            if (flipX)
                source.Width = -source.Width;
            if (flipY)
                source.Height = -source.Height;
            graphics.Pivot(transform, true);
            if (nPatchInfo.HasValue)
                graphics.DrawTextureNPatch(
                    texture,
                    nPatchInfo.Value,
                    source,
                    new Box(position, scale),
                    tint,
                    textureFilter,
                    textureWrap,
                    camera
                );
            else
                graphics.DrawTexture(
                    texture,
                    source,
                    new Box(position, scale),
                    tint,
                    textureFilter,
                    textureWrap,
                    camera
                );
        }
    }
}
