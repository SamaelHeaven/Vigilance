using Raylib_cs;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed unsafe class Texture : IDisposable
{
    private static Texture? _empty;
    private static Texture? _white;
    internal Vector2? LogicalSize;
    internal RenderTexture? RenderTexture;
    internal Texture2D Texture2D;

    internal Texture(in Texture2D texture2D, RenderTexture? renderTexture = null)
    {
        Game.ThrowIfNotRunning();
        Texture2D = texture2D;
        RenderTexture = renderTexture;
    }

    public Texture(string fileType, IEnumerable<byte> bytes)
    {
        Game.ThrowIfNotRunning();
        using var fileTypeBuffer = fileType.ToUtf8Ptr();
        var span = bytes.AsSpan();
        fixed (byte* bytesBuffer = span)
        {
            var image = Raylib.LoadImageFromMemory(fileTypeBuffer, bytesBuffer, span.Length);
            var logLevel = Log.SetLogLevel(Log.LogLevel.Max(LogLevel.Info));
            Texture2D = Raylib.LoadTextureFromImage(image);
            Log.LogLevel = logLevel;
            Raylib.UnloadImage(image);
        }
    }

    public static Texture Empty => _empty ??= new WritableImage<PixelGrayAlpha>(1, 1).ToTexture();

    public static Texture White => _white ??= new WritableImage<PixelGrayscale>(1, 1, Color.White).ToTexture();

    public uint Id => Texture2D.Id;

    public int Width => LogicalSize is { } size ? (int)size.X : Texture2D.Width;

    public int Height => LogicalSize is { } size ? (int)size.Y : Texture2D.Height;

    public Vector2 Size => new(Width, Height);

    public int PhysicalWidth => RenderTexture?.PhysicalWidth ?? Texture2D.Width;

    public int PhysicalHeight => RenderTexture?.PhysicalHeight ?? Texture2D.Height;

    public Vector2 PhysicalSize => new(PhysicalWidth, PhysicalHeight);

    public PixelFormat Format => (PixelFormat)Texture2D.Format;

    public bool IsValid => Texture2D.Id != 0;

    public bool IsRenderTexture => RenderTexture is not null;

    public TextureFilter TextureFilter
    {
        get;
        set
        {
            if (field == value)
                return;
            field = value;
            Raylib.SetTextureFilter(Texture2D, (Raylib_cs.TextureFilter)value);
        }
    } = Drawing.DefaultTextureFilter;

    public TextureWrap TextureWrap
    {
        get;
        set
        {
            if (field == value)
                return;
            field = value;
            Raylib.SetTextureWrap(Texture2D, (Raylib_cs.TextureWrap)value);
        }
    } = Drawing.DefaultTextureWrap;

    public void Dispose()
    {
        if (Id <= 1 || this == _empty || this == _white)
            return;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
        RenderTexture = null;
        Texture2D = default;
    }

    public WritableImage ToImage()
    {
        if (RenderTexture is not null && Graphics.IsBufferCurrent(RenderTexture))
            Graphics.DrawCurrentBuffer();
        var image = new WritableImage(new Image(Raylib.LoadImageFromTexture(Texture2D)));
        if (LogicalSize is { } size && (image.Width != (int)size.X || image.Height != (int)size.Y))
            image.Crop(0, 0, (int)size.X, (int)size.Y);
        if (RenderTexture is not null)
            image.FlipVertically();
        return image;
    }

    public WritableImage<T> ToImage<T>()
        where T : unmanaged, IPixel
    {
        return new WritableImage<T>(ToImage());
    }

    public WritableTexture Copy()
    {
        var image = ToImage();
        return new WritableTexture(image.ToTexture());
    }

    private void ReleaseUnmanagedResources()
    {
        if (RenderTexture is not null)
            Raylib.UnloadRenderTexture(RenderTexture.RenderTexture2D);
        else
            Raylib.UnloadTexture(Texture2D);
    }

    ~Texture()
    {
        Game.Defer(ReleaseUnmanagedResources);
    }
}
