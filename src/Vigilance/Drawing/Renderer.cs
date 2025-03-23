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
        _buffer = new WritableTexture(Game.Size);
        _interpolation = Game.DefaultInterpolation;
        _graphics = new Graphics(_buffer);
        Raylib.SetTextureFilter(_buffer.RenderTexture2D.Texture, (TextureFilter)_interpolation);
        Raylib.BeginTextureMode(_buffer.RenderTexture2D);
    }

    public static Interpolation Interpolation
    {
        get => GetRenderer()._interpolation;
        set
        {
            var renderer = GetRenderer();
            if (renderer._interpolation == value)
                return;
            renderer._interpolation = value;
            Raylib.SetTextureFilter(renderer._buffer.RenderTexture2D.Texture, (TextureFilter)value);
        }
    }

    public static Graphics Graphics => GetRenderer()._graphics;

    public static WritableTexture Buffer => GetRenderer()._buffer;

    internal static void Update()
    {
        var renderer = GetRenderer();
        var screenWidth = (float)Game.ScreenWidth;
        var screenHeight = (float)Game.ScreenHeight;
        var width = (float)Game.Width;
        var height = (float)Game.Height;
        var scale = MathF.Min(screenWidth / width, screenHeight / height);
        var buffer = renderer._buffer;
        var source = new Raylib_cs.Rectangle(0, 0, width, -height);
        var dest = new Raylib_cs.Rectangle(
            (screenWidth - width * scale) * 0.5f,
            (screenHeight - height * scale) * 0.5f,
            width * scale,
            height * scale
        );
        Raylib.EndTextureMode();
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Raylib_cs.Color.Black);
        Raylib.DrawTexturePro(buffer.RenderTexture2D.Texture, source, dest, Vector2.Zero, 0, Raylib_cs.Color.White);
        Raylib.EndDrawing();
        Raylib.BeginTextureMode(buffer.RenderTexture2D);
    }

    private static Renderer GetRenderer()
    {
        return _renderer ??= new Renderer();
    }
}
