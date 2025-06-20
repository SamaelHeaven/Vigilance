using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Renderer
{
    private static Renderer? _renderer;
    private readonly Graphics _graphics;
    private readonly WritableTexture? _buffer;
    private Vector2 _scale;
    private Vector2 _offset;

    private Renderer()
    {
        Game.EnsureRunning();
        var config = Game.RendererConfig;
        if (config.Mode == RenderingMode.Buffer)
            _buffer = new WritableTexture(Game.Size, config.Scale);
        _graphics = new Graphics(_buffer);
    }

    public static Graphics Graphics => GetRenderer()._graphics;

    public static WritableTexture? Buffer => GetRenderer()._buffer;

    internal static void BeginDrawing()
    {
        Graphics.Reset();
        Raylib.ClearBackground(Raylib_cs.Color.Black);
        var renderer = GetRenderer();
        var screenWidth = (float)Game.ScreenWidth;
        var screenHeight = (float)Game.ScreenHeight;
        var width = Game.Width;
        var height = Game.Height;
        var scaleX = screenWidth / width;
        var scaleY = screenHeight / height;
        var minScale = MathF.Min(scaleX, scaleY);
        var maxScale = MathF.Max(scaleX, scaleY);
        renderer._scale = Game.Viewport switch
        {
            Viewport.Fit => new Vector2(minScale),
            Viewport.Stretch => new Vector2(scaleX, scaleY),
            Viewport.Crop => new Vector2(maxScale),
            _ => throw new ArgumentOutOfRangeException(),
        };
        renderer._offset = (
            Game.Viewport switch
            {
                Viewport.Fit => new Vector2(
                    (screenWidth - width * minScale) * 0.5f,
                    (screenHeight - height * minScale) * 0.5f
                ),
                Viewport.Stretch => Vector2.Zero,
                Viewport.Crop => new Vector2(
                    (screenWidth - width * maxScale) * 0.5f,
                    (screenHeight - height * maxScale) * 0.5f
                ),
                _ => throw new ArgumentOutOfRangeException(),
            }
        ).Round();

        if (Game.RendererConfig.Mode == RenderingMode.Buffer)
            return;
        Graphics.Scale(renderer._scale);
        Graphics.Translate(renderer._offset);
    }

    internal static void EndDrawing()
    {
        var renderer = GetRenderer();
        var screenWidth = Game.ScreenWidth;
        var screenHeight = Game.ScreenHeight;
        var width = Game.Width;
        var height = Game.Height;
        var scaleX = renderer._scale.X;
        var scaleY = renderer._scale.Y;
        var offsetX = (int)renderer._offset.X;
        var offsetY = (int)renderer._offset.Y;
        var color = Color.Black;
        var config = Game.RendererConfig;
        Graphics.Reset();
        if (config.Mode == RenderingMode.Buffer)
        {
            var texture = renderer._buffer!.Texture.Texture2D;
            var source = new Raylib_cs.Rectangle(0, 0, texture.Width, -texture.Height);
            var dest = new Raylib_cs.Rectangle(offsetX, offsetY, width * scaleX, height * scaleY);
            Raylib.SetTextureFilter(texture, (TextureFilter)config.Interpolation);
            Raylib.DrawTexturePro(texture, source, dest, Vector2.Zero, 0, Raylib_cs.Color.White);
        }
        else
        {
            Graphics.FillRectangle(0, 0, offsetX, screenHeight, color);
            Graphics.FillRectangle(screenWidth - offsetX, 0, offsetX, screenHeight, color);
            Graphics.FillRectangle(0, 0, screenWidth, offsetY, color);
            Graphics.FillRectangle(0, screenHeight - offsetY, screenWidth, offsetY, color);
        }

        Graphics.DrawRenderBatchActive();
        Raylib.SwapScreenBuffer();
    }

    private static Renderer GetRenderer()
    {
        return _renderer ??= new Renderer();
    }
}
