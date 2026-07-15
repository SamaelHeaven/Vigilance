using System.ComponentModel;
using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public static class Renderer
{
    private static RenderTexture? _buffer;
    private static Vector2 _offset;

    static Renderer()
    {
        Game.ThrowIfNotRunning();
        var mode = Display.RenderingMode;
        Graphics = Display.Graphics = new Graphics(null, true);
        if (mode.Type != RenderingModeType.Buffer)
            return;
        _buffer = new RenderTexture(Display.ScreenSize, mode.Scale);
        Graphics = new Graphics(_buffer, true);
    }

    public static Graphics Graphics { get; }

    public static Vector2 Offset => _offset;

    public static Vector2 Scale { get; private set; }

    internal static void BeginDrawing()
    {
        var mode = Display.RenderingMode;
        if (
            mode.Type == RenderingModeType.Buffer
            && (_buffer is null || _buffer.ScaledSize != (Display.ScreenSize * mode.Scale).Floor())
        )
        {
            _buffer?.Dispose();
            _buffer = new RenderTexture(Display.ScreenSize, mode.Scale);
            Graphics.Buffer = _buffer;
        }
        else if (mode.Type != RenderingModeType.Buffer)
        {
            _buffer?.Dispose();
            _buffer = null;
            Graphics.Buffer = null;
        }

        Graphics.ResetCurrentBuffer();
        Graphics.ClearBackground(Display.Background);
        var screenWidth = (float)Display.ScreenWidth;
        var screenHeight = (float)Display.ScreenHeight;
        var width = Display.Width;
        var height = Display.Height;
        var scaleX = screenWidth / width;
        var scaleY = screenHeight / height;
        var minScale = scaleX.Min(scaleY);
        var maxScale = scaleX.Max(scaleY);
        var viewport = Display.Viewport;
        Scale = viewport switch
        {
            Viewport.Fit => new Vector2(minScale),
            Viewport.Stretch => new Vector2(scaleX, scaleY),
            Viewport.Crop => new Vector2(maxScale),
            Viewport.Native => Vector2.One,
            _ => throw new InvalidEnumArgumentException(
                $"{nameof(Game)}.{nameof(Display.Viewport)}",
                (int)viewport,
                typeof(Viewport)
            ),
        };
        _offset = (
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
                Viewport.Native => Vector2.Zero,
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
        var screenWidth = Display.ScreenWidth;
        var screenHeight = Display.ScreenHeight;
        var offsetX = (int)_offset.X;
        var offsetY = (int)_offset.Y;
        var mode = Display.RenderingMode;
        Graphics.ResetCurrentBuffer();
        if (_buffer is not null)
        {
            var texture = _buffer.Texture;
            texture.TextureFilter = mode.TextureFilter;
            var action = Game.Scene.DrawScreenAction;
            if (action is null)
            {
                var source = new Raylib_cs.Rectangle(0, 0, texture.Width, -texture.Height);
                var dest = new Raylib_cs.Rectangle(0, 0, screenWidth, screenHeight);
                Raylib.DrawTexturePro(texture.Texture2D, source, dest, Vector2.Zero, 0, Raylib_cs.Color.White);
            }
            else
            {
                var dest = new Box(0, 0, screenWidth, screenHeight);
                action.SafeInvoke(Display.Graphics, texture, dest);
                Debug.Assert(texture.IsValid);
            }
        }

        var background = Display.Background.RColor;
        Graphics.ResetCurrentBuffer();
        Raylib.DrawRectangle(0, 0, offsetX, screenHeight, background);
        Raylib.DrawRectangle(screenWidth - offsetX, 0, offsetX, screenHeight, background);
        Raylib.DrawRectangle(0, 0, screenWidth, offsetY, background);
        Raylib.DrawRectangle(0, screenHeight - offsetY, screenWidth, offsetY, background);
        Graphics.DrawCurrentBuffer();
        Raylib.SwapScreenBuffer();
    }
}
