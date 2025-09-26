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

    public static Texture Empty => _empty ??= new WritableImage<PixelGrayAlpha>(1, 1).ToTexture();

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
