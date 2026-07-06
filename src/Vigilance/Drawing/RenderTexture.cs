using Raylib_cs;
using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class RenderTexture : IDisposable
{
    private readonly bool _pool;
    private bool _pooled;
    internal RenderTexture2D RenderTexture2D;

    public RenderTexture(Vector2 size, float scale = 1, bool pool = true)
        : this(size.X, size.Y, scale, pool) { }

    public RenderTexture(float width, float height, float scale = 1, bool pool = true)
    {
        Game.ThrowIfNotRunning();
        _pool = pool;
        Scale = scale.Max(1);
        var scaledWidth = (int)(width * Scale).Max(1);
        var scaledHeight = (int)(height * Scale).Max(1);
        bool rented;
        if (
            _pool
            && RenderTexturePool.TryRent(
                scaledWidth,
                scaledHeight,
                out var physical,
                out var physicalWidth,
                out var physicalHeight
            )
        )
        {
            rented = true;
            PhysicalWidth = physicalWidth;
            PhysicalHeight = physicalHeight;
        }
        else
        {
            Graphics.ResetCurrentBuffer();
            var multipleOf = Drawing.RenderTexturePoolRoundUpToMultipleOf;
            PhysicalWidth = _pool ? scaledWidth.RoundUpToMultipleOf(multipleOf) : scaledWidth;
            PhysicalHeight = _pool ? scaledHeight.RoundUpToMultipleOf(multipleOf) : scaledHeight;
            var logLevel = Log.SetLogLevel(Log.LogLevel.Max(LogLevel.Info));
            physical = Raylib.LoadRenderTexture(PhysicalWidth, PhysicalHeight);
            Log.LogLevel = logLevel;
            rented = false;
        }

        RenderTexture2D = physical;
        RenderTexture2D.Texture.Width = scaledWidth;
        RenderTexture2D.Texture.Height = scaledHeight;
        Texture = new Texture(physical.Texture, this) { LogicalSize = new Vector2(scaledWidth, scaledHeight) };
        Graphics = new Graphics(this);
        if (rented)
            Graphics.ClearBackground(Color.Transparent);
    }

    public int PhysicalWidth { get; }
    public int PhysicalHeight { get; }
    public Texture Texture { get; private set; }
    public Graphics Graphics { get; private set; }
    public float Scale { get; private set; }

    public float Width => RenderTexture2D.Texture.Width / Scale;

    public float Height => RenderTexture2D.Texture.Height / Scale;

    public Vector2 Size => new(Width, Height);

    public Vector2 PhysicalSize => new(PhysicalWidth, PhysicalHeight);

    public int ScaledWidth => RenderTexture2D.Texture.Width;

    public int ScaledHeight => RenderTexture2D.Texture.Height;

    public Vector2 ScaledSize => new(ScaledWidth, ScaledHeight);

    public PixelFormat Format => (PixelFormat)RenderTexture2D.Texture.Format;

    public bool IsValid => RenderTexture2D.Texture.Id != 0;

    public void Dispose()
    {
        Dispose(_pool);
    }

    public void Dispose(bool pool)
    {
        if (_pooled || !IsValid)
            return;
        if (pool)
        {
            _pooled = true;
            RenderTexture2D.Texture.Width = PhysicalWidth;
            RenderTexture2D.Texture.Height = PhysicalHeight;
            Graphics = null!;
            RenderTexturePool.Return(this);
        }
        else
        {
            Texture.Dispose();
            RenderTexture2D = default;
            Texture = Texture.Empty;
            Graphics = null!;
            Scale = 0;
        }
    }

    public static implicit operator Texture(RenderTexture renderTexture)
    {
        return renderTexture.Texture;
    }

    public WritableImage<PixelR8G8B8A8> ToImage(TextureFilter? textureFilter = null)
    {
        var image = new WritableImage<PixelR8G8B8A8>(Texture.ToImage());
        if (Precision.AreEqual(Scale, 1))
            return image;
        image.Resize(Size, textureFilter);
        return image;
    }

    public WritableImage<PixelR8G8B8A8> ToScaledImage()
    {
        return new WritableImage<PixelR8G8B8A8>(Texture.ToImage());
    }

    internal void DetachForReuse(out RenderTexture2D renderTexture2D, out int physicalWidth, out int physicalHeight)
    {
        renderTexture2D = RenderTexture2D;
        physicalWidth = PhysicalWidth;
        physicalHeight = PhysicalHeight;
        RenderTexture2D = default;
    }
}
