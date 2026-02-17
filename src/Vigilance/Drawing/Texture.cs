using Raylib_cs.BleedingEdge;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed unsafe class Texture : IDisposable
{
    private static Texture? _empty;
    private static Texture? _white;
    internal RenderTexture? RenderTexture;
    internal Texture2D Texture2D;

    internal Texture(in Texture2D texture2D, RenderTexture? renderTexture = null)
    {
        Game.EnsureRunning();
        Texture2D = texture2D;
        RenderTexture = renderTexture;
    }

    public Texture(string fileType, IEnumerable<byte> bytes)
    {
        Game.EnsureRunning();
        using var fileTypeBuffer = fileType.ToUtf8Buffer();
        var span = bytes.AsSpan();
        fixed (byte* bytesBuffer = span)
        {
            var image = Raylib.LoadImageFromMemory(fileTypeBuffer, bytesBuffer, span.Length);
            Texture2D = Raylib.LoadTextureFromImage(image);
            Raylib.UnloadImage(image);
        }
    }

    public static Texture Empty => _empty ??= new WritableImage<PixelGrayAlpha>(1, 1).ToTexture();

    public static Texture White =>
        _white ??= new Texture(
            new Texture2D
            {
                Id = 1,
                Width = 1,
                Height = 1,
                Format = Raylib_cs.BleedingEdge.PixelFormat.UncompressedR8G8B8A8,
                Mipmaps = 1,
            }
        );

    public uint Id => Texture2D.Id;

    public int Width => Texture2D.Width;

    public int Height => Texture2D.Height;

    public Vector2 Size => new(Width, Height);

    public PixelFormat Format => (PixelFormat)Texture2D.Format;

    public bool IsValid => Texture2D.Id != 0;

    public void Dispose()
    {
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
