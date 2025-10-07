using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed unsafe class Texture
{
    private static Texture? _empty;
    private static Texture? _white;
    internal readonly RenderTexture? RenderTexture;
    internal readonly Texture2D Texture2D;

    internal Texture(Texture2D texture2D, RenderTexture? renderTexture = null)
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
            var image = Raylib.LoadImageFromMemory(fileTypeBuffer.AsPointer(), bytesBuffer, span.Length);
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

    ~Texture()
    {
        Game.Defer(() =>
        {
            if (RenderTexture is null)
                Raylib.UnloadTexture(Texture2D);
            else
                Raylib.UnloadRenderTexture(RenderTexture.RenderTexture2D);
        });
    }
}
