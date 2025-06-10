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
        var width = Game.Width;
        var height = Game.Height;
        var texture = renderer._buffer.RenderTexture2D.Texture;
        var scaleX = screenWidth / width;
        var scaleY = screenHeight / height;
        var minScale = MathF.Min(scaleX, scaleY);
        var maxScale = MathF.Max(scaleX, scaleY);
        var source = new Raylib_cs.Rectangle(0, 0, texture.Width, -texture.Height);
        var dest = Game.Viewport switch
        {
            Viewport.Fit => new Raylib_cs.Rectangle(
                (screenWidth - width * minScale) * 0.5f,
                (screenHeight - height * minScale) * 0.5f,
                width * minScale,
                height * minScale
            ),
            Viewport.Stretch => new Raylib_cs.Rectangle(0, 0, screenWidth, screenHeight),
            Viewport.Crop => new Raylib_cs.Rectangle(
                (screenWidth - width * maxScale) * 0.5f,
                (screenHeight - height * maxScale) * 0.5f,
                width * maxScale,
                height * maxScale
            ),
            _ => throw new ArgumentOutOfRangeException(),
        };

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
