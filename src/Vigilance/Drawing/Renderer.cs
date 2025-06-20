using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Renderer
{
    private static Renderer? _renderer;
    private readonly Graphics _graphics;
    private Vector2 _offset;

    private Renderer()
    {
        Game.EnsureRunning();
        _graphics = new Graphics(null);
    }

    public static Graphics Graphics => GetRenderer()._graphics;

    internal static void BeginDrawing()
    {
        Graphics.Reset();
        Graphics.ClearBackground(Color.Black);
        var renderer = GetRenderer();
        var screenWidth = (float)Game.ScreenWidth;
        var screenHeight = (float)Game.ScreenHeight;
        var width = Game.Width;
        var height = Game.Height;
        var scaleX = screenWidth / width;
        var scaleY = screenHeight / height;
        var minScale = MathF.Min(scaleX, scaleY);
        var maxScale = MathF.Max(scaleX, scaleY);
        var scale = Game.Viewport switch
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
        Graphics.Scale(scale);
        Graphics.Translate(renderer._offset);
    }

    internal static void EndDrawing()
    {
        var renderer = GetRenderer();
        var screenWidth = Game.ScreenWidth;
        var screenHeight = Game.ScreenHeight;
        var offsetX = (int)renderer._offset.X;
        var offsetY = (int)renderer._offset.Y;
        var color = Color.Black;
        Graphics.Reset();
        Graphics.FillRectangle(0, 0, offsetX, screenHeight, color);
        Graphics.FillRectangle(screenWidth - offsetX, 0, offsetX, screenHeight, color);
        Graphics.FillRectangle(0, 0, screenWidth, offsetY, color);
        Graphics.FillRectangle(0, screenHeight - offsetY, screenWidth, offsetY, color);
        Graphics.DrawRenderBatchActive();
        Raylib.SwapScreenBuffer();
    }

    private static Renderer GetRenderer()
    {
        return _renderer ??= new Renderer();
    }
}
