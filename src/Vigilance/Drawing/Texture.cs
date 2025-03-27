using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Texture
{
    private readonly object? _owner;
    internal readonly Texture2D Texture2D;

    internal Texture(Texture2D texture2D, object owner)
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
