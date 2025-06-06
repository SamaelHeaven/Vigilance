using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Renderer
{
    private static Renderer? _renderer;
    private readonly WritableTexture _buffer;
    private readonly Graphics _graphics;
    private Interpolation _interpolation;

    private Renderer()
    {
        Game.EnsureRunning();
        _buffer = new WritableTexture(Game.Size, Game.Scale);
        _interpolation = Game.DefaultInterpolation;
        _graphics = _buffer.Graphics;
    }

    public static Interpolation Interpolation
    {
        get => GetRenderer()._interpolation;
        set => GetRenderer()._interpolation = value;
    }

    public static Graphics Graphics => GetRenderer()._graphics;

    public static WritableTexture Buffer => GetRenderer()._buffer;

    internal static void Update()
    {
        var renderer = GetRenderer();
        var screenWidth = (float)Game.ScreenWidth;
        var screenHeight = (float)Game.ScreenHeight;
        var texture = renderer._buffer.RenderTexture2D.Texture;
        var width = renderer._buffer.UnscaledWidth;
        var height = renderer._buffer.UnscaledHeight;
        var scale = MathF.Min(screenWidth / width, screenHeight / height);
        var source = new Raylib_cs.Rectangle(0, 0, texture.Width, -texture.Height);
        var dest = new Raylib_cs.Rectangle(
            (screenWidth - width * scale) * 0.5f,
            (screenHeight - height * scale) * 0.5f,
            width * scale,
            height * scale
        );
        if (Graphics.CurrentBuffer is not null)
        {
            Raylib.EndTextureMode();
            Graphics.CurrentBuffer = null;
        }

        if (Graphics.CurrentClip.HasValue)
        {
            Raylib.EndScissorMode();
            Graphics.CurrentClip = null;
        }

        Raylib.SetTextureFilter(texture, (TextureFilter)renderer._interpolation);
        Raylib.ClearBackground(Raylib_cs.Color.Black);
        Raylib.DrawTexturePro(texture, source, dest, Vector2.Zero, 0, Raylib_cs.Color.White);
        Rlgl.DrawRenderBatchActive();
        Raylib.SwapScreenBuffer();
    }

    private static Renderer GetRenderer()
    {
        return _renderer ??= new Renderer();
    }
}
