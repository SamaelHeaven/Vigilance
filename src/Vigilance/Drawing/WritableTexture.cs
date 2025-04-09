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

    public int Width => RenderTexture2D.Texture.Width;

    public int Height => RenderTexture2D.Texture.Height;

    public Vector2 Size => new(Width, Height);

    public void UpdatePixels(Color[] colors)
    {
        if (Graphics.CurrentBuffer == this)
        {
            Raylib.EndTextureMode();
            Raylib.BeginTextureMode(RenderTexture2D);
        }

        Raylib.UpdateTexture(RenderTexture2D.Texture, colors);
    }

    public void UpdatePixels(ReadOnlySpan<Color> colors)
    {
        if (Graphics.CurrentBuffer == this)
        {
            Raylib.EndTextureMode();
            Raylib.BeginTextureMode(RenderTexture2D);
        }

        Raylib.UpdateTexture(RenderTexture2D.Texture, colors);
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
