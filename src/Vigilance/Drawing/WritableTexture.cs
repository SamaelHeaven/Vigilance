using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class WritableTexture
{
    internal readonly RenderTexture2D RenderTexture2D;
    public readonly Texture Texture;

    public WritableTexture(Vector2 size)
        : this((int)size.X, (int)size.Y) { }

    public WritableTexture(int width, int height)
    {
        Game.EnsureRunning();
        RenderTexture2D = Raylib.LoadRenderTexture(width, height);
        Texture = new Texture(RenderTexture2D.Texture, this);
    }

    public static implicit operator Texture(WritableTexture writableTexture)
    {
        return writableTexture.Texture;
    }

    ~WritableTexture()
    {
        Game.RunLater(() =>
        {
            Raylib.UnloadRenderTexture(RenderTexture2D);
        });
    }
}
