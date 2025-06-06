using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class WritableTexture
{
    internal readonly RenderTexture2D RenderTexture2D;

    public WritableTexture(Vector2 size, float scale = 1)
        : this(size.X, size.Y, scale) { }

    public WritableTexture(float width, float height, float scale = 1)
    {
        Game.EnsureRunning();
        RenderTexture2D = Raylib.LoadRenderTexture((int)(width * scale), (int)(height * scale));
        Texture = new Texture(RenderTexture2D.Texture, this);
        Graphics = new Graphics(this);
        Scale = MathF.Max(1, scale);
    }

    public Texture Texture { get; }
    public Graphics Graphics { get; }
    public float Scale { get; }

    public float Width => RenderTexture2D.Texture.Width / Scale;

    public float Height => RenderTexture2D.Texture.Height / Scale;

    public Vector2 Size => new(Width, Height);

    public int ScaledWidth => RenderTexture2D.Texture.Width;

    public int ScaledHeight => RenderTexture2D.Texture.Height;

    public Vector2 ScaledSize => new(ScaledWidth, ScaledHeight);

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
        Game.Defer(() =>
        {
            Raylib.UnloadRenderTexture(RenderTexture2D);
        });
    }
}
