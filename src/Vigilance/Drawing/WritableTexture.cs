using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class WritableTexture
{
    internal readonly RenderTexture2D RenderTexture2D;

    public WritableTexture(Vector2 size)
        : this((int)size.X, (int)size.Y) { }

    public WritableTexture(int width, int height)
    {
        Game.EnsureRunning();
        RenderTexture2D = Raylib.LoadRenderTexture(width, height);
        Texture = new Texture(RenderTexture2D.Texture, this);
        Graphics = new Graphics(this);
    }

    public Texture Texture { get; }
    public Graphics Graphics { get; }

    public int Width => RenderTexture2D.Texture.Width;

    public int Height => RenderTexture2D.Texture.Height;

    public Vector2 Size => new(Width, Height);

    public void Update(ReadOnlySpan<Color> pixels)
    {
        if (Graphics.CurrentBuffer == this)
            Rlgl.DrawRenderBatchActive();
        Raylib.UpdateTexture(RenderTexture2D.Texture, pixels);
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
