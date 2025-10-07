using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class RenderTexture
{
    internal readonly RenderTexture2D RenderTexture2D;

    public RenderTexture(Vector2 size, float scale = 1)
        : this(size.X, size.Y, scale) { }

    public RenderTexture(float width, float height, float scale = 1)
    {
        Game.EnsureRunning();
        Graphics.Reset();
        Scale = scale.Max(1);
        RenderTexture2D = Raylib.LoadRenderTexture((int)(width * Scale), (int)(height * Scale));
        Texture = new Texture(RenderTexture2D.Texture, this);
        Graphics = new Graphics(this);
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

    public static implicit operator Texture(RenderTexture renderTexture)
    {
        return renderTexture.Texture;
    }

    public WritableImage<PixelR8G8B8A8> ToImage(Interpolation? interpolation = null)
    {
        var image = new WritableImage<PixelR8G8B8A8>(Texture.ToImage());
        if (Precision.AreEqual(Scale, 1))
            return image;
        image.Resize(Size, interpolation);
        return image;
    }

    public WritableImage<PixelR8G8B8A8> ToScaledImage()
    {
        return new WritableImage<PixelR8G8B8A8>(Texture.ToImage());
    }
}
