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

    public Texture(string path)
    {
        Game.EnsureRunning();
        path = FileSystem.FormatPath(path);
        if (!FileSystem.FileExists(path))
            throw new ArgumentException($"Could not find texture file '{path}'.");
        Texture2D = Raylib.LoadTexture(path);
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
