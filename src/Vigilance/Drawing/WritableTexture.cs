using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed unsafe class WritableTexture
{
    internal readonly RenderTexture2D RenderTexture2D;

    public WritableTexture(Vector2 size, float scale = 1)
        : this(size.X, size.Y, scale) { }

    public WritableTexture(float width, float height, float scale = 1)
    {
        Game.EnsureRunning();
        RenderTexture2D = Raylib.LoadRenderTexture((int)(width * scale), (int)(height * scale));
        Texture = new Texture(RenderTexture2D.Texture, this);
        Graphics = new Graphics(this);
        Scale = scale.Max(1);
    }

    public Texture Texture { get; }
    public Graphics Graphics { get; }
    public float Scale { get; }

    public float Width => RenderTexture2D.Texture.Width / Scale;

    public float Height => RenderTexture2D.Texture.Height / Scale;

    public Vector2 Size => new(Width, Height);

    public int ScaledWidth => RenderTexture2D.Texture.Width;

    public int ScaledHeight => RenderTexture2D.Texture.Height;

    public Vector2 ScaledSize => new(ScaledWidth, ScaledHeight);

    public PixelFormat Format => (PixelFormat)RenderTexture2D.Texture.Format;

    public void Update(Image image, Box? box = null)
    {
        Update(new ReadOnlySpan<PixelGrayscale>(image.RImage.Data, image.DataSize), box);
    }

    public void Update(WritableImage image, Box? box = null)
    {
        Update((Image)image, box);
    }

    public void Update<T>(WritableImage<T> image, Box? box = null)
        where T : unmanaged, IPixel
    {
        Update((ReadOnlySpan<T>)image.AsSpan(), box);
    }

    public void Update<T>(ReadOnlySpan<T> pixels, Box? box = null)
        where T : unmanaged, IPixel
    {
        if (Graphics.IsBufferCurrent(this))
            Graphics.DrawCurrentBuffer();
        var source = box ?? new Box(Vector2.Zero, ScaledSize);
        Raylib.UpdateTextureRec(
            RenderTexture2D.Texture,
            new Raylib_cs.BleedingEdge.Rectangle(source.Position, source.Size),
            pixels
        );
    }

    public static implicit operator Texture(WritableTexture writableTexture)
    {
        return writableTexture.Texture;
    }

    public WritableImage ToImage(Interpolation? interpolation = null)
    {
        var image = Texture.ToImage();
        if (Precision.AreEqual(Scale, 1))
            return image;
        image.Resize(Width, Height, interpolation);
        return image;
    }

    public WritableImage ToScaledImage()
    {
        return Texture.ToImage();
    }
}
