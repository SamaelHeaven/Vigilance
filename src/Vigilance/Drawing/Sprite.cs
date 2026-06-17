using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Sprite : Drawable<Sprite>
{
    public Sprite() { }

    public Sprite(Texture texture)
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
    public Interpolation Interpolation { get; set; } = Drawing.DefaultInterpolation;

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
    }

    protected override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawSprite(transform, this);
    }
}

public static class SpriteExtensions
{
    extension(Graphics graphics)
    {
        public void DrawSprite(Sprite sprite)
        {
            graphics.DrawSprite(new Transform(), sprite);
        }

        public void DrawSprite(float x, float y, float width, float height, Sprite sprite)
        {
            graphics.DrawSprite(new Vector2(x, y), new Vector2(width, height), sprite);
        }

        public void DrawSprite(Vector2 position, Vector2 size, Sprite sprite)
        {
            graphics.DrawSprite(new Transform(position + size * 0.5f, size), sprite);
        }

        public void DrawSprite(in Box box, Sprite sprite)
        {
            graphics.DrawSprite(box.Position, box.Size, sprite);
        }

        public void DrawSprite(Transform transform, Sprite sprite)
        {
            sprite.OnBeginDrawing?.Invoke(transform, sprite, graphics);
            transform += sprite.Transform;
            var camera = sprite.Camera.Get();
            var texture = sprite.Texture;
            var interpolation = sprite.Interpolation;
            var tint = sprite.Tint;
            var nPatchInfo = sprite.NPatchInfo;
            var flipX = sprite.FlipX;
            var flipY = sprite.FlipY;
            var position = transform.Position;
            var scale = transform.Scale.Abs();
            var source = sprite.Source ?? new Box(Vector2.Zero, new Vector2(texture.Width, texture.Height));
            if (flipX)
                source.Width = -source.Width;
            if (flipY)
                source.Height = -source.Height;
            graphics.PushMatrix();
            graphics.Pivot(transform, true);
            if (nPatchInfo.HasValue)
                graphics.DrawTextureNPatch(
                    texture,
                    nPatchInfo.Value,
                    source,
                    new Box(position, scale),
                    tint,
                    interpolation,
                    camera
                );
            else
                graphics.DrawTexture(texture, source, new Box(position, scale), tint, interpolation, camera);
            graphics.PopMatrix();
            sprite.OnEndDrawing?.Invoke(transform, sprite, graphics);
        }
    }
}
