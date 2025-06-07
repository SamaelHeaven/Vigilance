using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed unsafe class Texture
{
    private static Texture? _empty;
    private readonly object? _owner;
    internal readonly Texture2D Texture2D;

    internal Texture(Texture2D texture2D, object? owner = null)
    {
        Game.EnsureRunning();
        Texture2D = texture2D;
        _owner = owner;
    }

    public Texture(string fileType, ReadOnlySpan<byte> bytes)
    {
        Game.EnsureRunning();
        using var fileTypeBuffer = fileType.ToUtf8Buffer();
        fixed (byte* bytesBuffer = bytes)
        {
            var image = Raylib.LoadImageFromMemory(fileTypeBuffer.AsPointer(), bytesBuffer, bytes.Length);
            Texture2D = Raylib.LoadTextureFromImage(image);
            Raylib.UnloadImage(image);
        }
    }

    public Texture(ReadOnlySpan<Color> pixels, int width, int height)
    {
        Game.EnsureRunning();
        if (pixels.Length != width * height)
            throw new ArgumentException("Pixels length must be equal to width * height.");
        var result = new Texture2D
        {
            Width = width,
            Height = height,
            Format = PixelFormat.UncompressedR8G8B8A8,
            Mipmaps = 1,
        };
        fixed (Color* pixelsBuffer = pixels)
        {
            result.Id = Rlgl.LoadTexture(pixelsBuffer, result.Width, result.Height, result.Format, result.Mipmaps);
        }

        Texture2D = result;
    }

    public static Texture Empty => _empty ??= new Texture(stackalloc Color[1] { Color.Transparent }, 1, 1);

    public uint Id => Texture2D.Id;

    public int Width => Texture2D.Width;

    public int Height => Texture2D.Height;

    public Vector2 Size => new(Width, Height);

    public bool Writable => _owner is WritableTexture;

    public Image ToImage()
    {
        var buffer = Graphics.CurrentBuffer;
        if (_owner is not null && buffer == _owner)
            Rlgl.DrawRenderBatchActive();
        var image = Raylib.LoadImageFromTexture(Texture2D);
        if (Writable)
            Raylib.ImageFlipVertical(ref image);
        return new Image(image);
    }

    public Texture Copy()
    {
        var image = ToImage();
        return image.ToTexture();
    }

    ~Texture()
    {
        if (_owner is not null)
            return;
        Game.Defer(() =>
        {
            Raylib.UnloadTexture(Texture2D);
        });
    }
}
