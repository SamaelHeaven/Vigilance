using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Texture
{
    private readonly object? _owner;
    internal readonly Texture2D Texture2D;

    internal Texture(Texture2D texture2D, object? owner = null)
    {
        Game.EnsureRunning();
        Texture2D = texture2D;
        _owner = owner;
    }

    public Texture(string fileType, byte[] bytes)
    {
        Game.EnsureRunning();
        var image = Raylib.LoadImageFromMemory(fileType, bytes);
        Texture2D = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);
    }

    public int Width => Texture2D.Width;

    public int Height => Texture2D.Height;

    public Vector2 Size => new(Width, Height);

    public bool Writable => _owner is WritableTexture;

    public Image ToImage(Interpolation? interpolation = null)
    {
        var buffer = Graphics.CurrentBuffer;
        if (_owner != null && buffer == _owner)
        {
            Raylib.EndTextureMode();
            Raylib.BeginTextureMode(buffer.RenderTexture2D);
        }

        Raylib.SetTextureFilter(Texture2D, (TextureFilter)(interpolation ?? Game.DefaultInterpolation));
        var image = Raylib.LoadImageFromTexture(Texture2D);
        if (Writable)
            Raylib.ImageFlipVertical(ref image);
        return new Image(image);
    }

    ~Texture()
    {
        if (_owner != null)
            return;
        Game.RunLater(() =>
        {
            Raylib.UnloadTexture(Texture2D);
        });
    }
}
