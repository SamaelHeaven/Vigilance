using System.ComponentModel;
using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Renderer
{
    private static Renderer? _renderer;
    private readonly RenderTexture? _buffer;
    private readonly Graphics _graphics;
    private Vector2 _offset;
    private Vector2 _scale;

    private Renderer()
    {
        Game.EnsureRunning();
        var mode = Display.RenderingMode;
        if (mode.ModeType == RenderingModeType.Buffer)
            _buffer = new RenderTexture(Display.Size, mode.Scale);
        _graphics = new Graphics(_buffer);
    }

    public static Graphics Graphics => GetRenderer()._graphics;

    public static Vector2 Offset => GetRenderer()._offset;

    public static Vector2 Scale => GetRenderer()._scale;

    internal static void BeginDrawing()
    {
        Graphics.Reset();
        Raylib.ClearBackground(Display.Background.RColor);
        var renderer = GetRenderer();
        var screenWidth = (float)Display.ScreenWidth;
        var screenHeight = (float)Display.ScreenHeight;
        var width = Display.Width;
        var height = Display.Height;
        var scaleX = screenWidth / width;
        var scaleY = screenHeight / height;
        var minScale = scaleX.Min(scaleY);
        var maxScale = scaleX.Max(scaleY);
        var viewport = Display.Viewport;
        renderer._scale = viewport switch
        {
            Viewport.Fit => new Vector2(minScale),
            Viewport.Stretch => new Vector2(scaleX, scaleY),
            Viewport.Crop => new Vector2(maxScale),
            _ => throw new InvalidEnumArgumentException(
                $"{nameof(Game)}.{nameof(Display.Viewport)}",
                (int)viewport,
                typeof(Viewport)
            ),
        };
        renderer._offset = (
            Display.Viewport switch
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
                _ => throw new InvalidEnumArgumentException(
                    $"{nameof(Game)}.{nameof(Display.Viewport)}",
                    (int)viewport,
                    typeof(Viewport)
                ),
            }
        ).Ceil();
    }

    internal static void EndDrawing()
    {
        var renderer = GetRenderer();
        var screenWidth = Display.ScreenWidth;
        var screenHeight = Display.ScreenHeight;
        var width = Display.Width;
        var height = Display.Height;
        var scaleX = renderer._scale.X;
        var scaleY = renderer._scale.Y;
        var offsetX = (int)renderer._offset.X;
        var offsetY = (int)renderer._offset.Y;
        var background = Display.Background.RColor;
        var mode = Display.RenderingMode;
        Graphics.Reset();
        if (renderer._buffer is null)
        {
            Raylib.DrawRectangle(0, 0, offsetX, screenHeight, background);
            Raylib.DrawRectangle(screenWidth - offsetX, 0, offsetX, screenHeight, background);
            Raylib.DrawRectangle(0, 0, screenWidth, offsetY, background);
            Raylib.DrawRectangle(0, screenHeight - offsetY, screenWidth, offsetY, background);
        }
        else
        {
            var texture = renderer._buffer.Texture.Texture2D;
            var source = new Raylib_cs.BleedingEdge.Rectangle(0, 0, texture.Width, -texture.Height);
            var dest = new Raylib_cs.BleedingEdge.Rectangle(offsetX, offsetY, width * scaleX, height * scaleY);
            Raylib.SetTextureFilter(texture, (TextureFilter)mode.Interpolation);
            Raylib.DrawTexturePro(texture, source, dest, Vector2.Zero, 0, Raylib_cs.BleedingEdge.Color.White);
        }

        Graphics.DrawCurrentBuffer();
        Raylib.SwapScreenBuffer();
    }

    private static Renderer GetRenderer()
    {
        return _renderer ??= new Renderer();
    }
}
