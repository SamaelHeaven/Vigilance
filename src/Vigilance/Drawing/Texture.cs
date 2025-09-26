using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed unsafe class Texture
{
    private static Texture? _empty;
    private readonly WritableTexture? _writableTexture;
    internal readonly Texture2D Texture2D;

    internal Texture(Texture2D texture2D, WritableTexture? writableTexture = null)
    {
        Game.EnsureRunning();
        Texture2D = texture2D;
        _writableTexture = writableTexture;
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

    public Texture(IEnumerable<Color> pixels, int width, int height)
    {
        Game.EnsureRunning();
        var span = pixels.AsSpan();
        if (span.Length != width * height)
            throw new ArgumentException("Pixels length must be equal to width * height.");
        var result = new Texture2D
        {
            Width = width,
            Height = height,
            Format = Raylib_cs.BleedingEdge.PixelFormat.UncompressedR8G8B8A8,
            Mipmaps = 1,
        };
        fixed (Color* pixelsBuffer = span)
        {
            result.Id = Rlgl.LoadTexture(pixelsBuffer, result.Width, result.Height, result.Format, result.Mipmaps);
        }

        Texture2D = result;
    }

    public static Texture Empty => _empty ??= new Texture([Color.Transparent], 1, 1);

    public uint Id => Texture2D.Id;

    public int Width => Texture2D.Width;

    public int Height => Texture2D.Height;

    public Vector2 Size => new(Width, Height);

    public PixelFormat Format => (PixelFormat)Texture2D.Format;

    public bool Writable => _writableTexture is not null;

    public WritableImage ToImage()
    {
        if (_writableTexture is not null && Graphics.IsBufferCurrent(_writableTexture))
            Graphics.DrawCurrentBuffer();
        var image = Raylib.LoadImageFromTexture(Texture2D);
        if (Writable)
            Raylib.ImageFlipVertical(ref image);
        return new WritableImage(new Image(image));
    }

    public Texture Copy()
    {
        var image = ToImage();
        return image.ToTexture();
    }

    ~Texture()
    {
        Game.Defer(() =>
        {
            if (_writableTexture is null)
                Raylib.UnloadTexture(Texture2D);
            else
                Raylib.UnloadRenderTexture(_writableTexture.RenderTexture2D);
        });
    }
}
