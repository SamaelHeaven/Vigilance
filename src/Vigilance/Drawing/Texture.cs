using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Texture
{
    private readonly bool _cleanup;
    internal readonly Texture2D Texture2D;

    internal Texture(Texture2D texture2D, bool cleanup)
    {
        Game.EnsureRunning();
        Texture2D = texture2D;
        _cleanup = cleanup;
    }

    public Texture(string path)
    {
        Game.EnsureRunning();
        path = FileSystem.FormatPath(path);
        if (!FileSystem.FileExists(path))
            throw new ArgumentException($"Could not find texture file '{path}'.");
        Texture2D = Raylib.LoadTexture(path);
        _cleanup = true;
    }

    public int Width => Texture2D.Width;

    public int Height => Texture2D.Height;

    public Vector2 Size => new(Width, Height);

    ~Texture()
    {
        if (!_cleanup)
            return;
        Game.RunLater(() =>
        {
            Raylib.UnloadTexture(Texture2D);
        });
    }
}
