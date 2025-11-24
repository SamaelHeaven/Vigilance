using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public readonly unsafe struct WritableTexture : IDisposable
{
    private readonly Texture _texture;

    public uint Id => _texture.Id;

    public int Width => _texture.Width;

    public int Height => _texture.Height;

    public Vector2 Size => _texture.Size;

    public PixelFormat Format => _texture.Format;

    public bool IsValid => _texture.IsValid;

    internal WritableTexture(Texture texture)
    {
        _texture = texture;
    }

    public WritableTexture(string fileType, IEnumerable<byte> bytes)
        : this(new Texture(fileType, bytes)) { }

    public WritableTexture(int width, int height, PixelFormat format = PixelFormat.UncompressedR8G8B8A8)
    {
        Game.EnsureRunning();
        var id = Rlgl.LoadTexture(null, width, height, (Raylib_cs.BleedingEdge.PixelFormat)format, 1);
        var texture2D = new Texture2D
        {
            Id = id,
            Width = width,
            Height = height,
            Format = (Raylib_cs.BleedingEdge.PixelFormat)format,
            Mipmaps = 1,
        };
        _texture = new Texture(texture2D);
    }

    public static implicit operator Texture(WritableTexture writableTexture)
    {
        return writableTexture._texture;
    }

    public WritableImage ToImage()
    {
        return _texture.ToImage();
    }

    public WritableImage<T> ToImage<T>()
        where T : unmanaged, IPixel
    {
        return _texture.ToImage<T>();
    }

    public WritableTexture Copy()
    {
        return _texture.Copy();
    }

    public void Update(Image image, in Box? box = null)
    {
        Update(new ReadOnlySpan<PixelGrayscale>(image.RImage.Data, image.DataSize), box);
    }

    public void Update(WritableImage image, in Box? box = null)
    {
        Update((Image)image, box);
    }

    public void Update<T>(WritableImage<T> image, in Box? box = null)
        where T : unmanaged, IPixel
    {
        Update(image.AsSpan(), box);
    }

    public void Update<T>(in ReadOnlySpan<T> pixels, in Box? box = null)
        where T : unmanaged, IPixel
    {
        var source = box ?? new Box(Vector2.Zero, Size);
        Raylib.UpdateTextureRec(
            _texture.Texture2D,
            new Raylib_cs.BleedingEdge.Rectangle(source.Position, source.Size),
            pixels
        );
    }

    public void Dispose()
    {
        _texture.Dispose();
    }
}
