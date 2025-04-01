using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

public readonly struct Graphics
{
    internal static WritableTexture? CurrentBuffer;
    public readonly WritableTexture Buffer;

    public Graphics(WritableTexture buffer)
    {
        Game.EnsureRunning();
        Buffer = buffer;
    }

    public static void PushState()
    {
        Game.EnsureRunning();
        Rlgl.PushMatrix();
    }

    public static void PopState()
    {
        Game.EnsureRunning();
        Rlgl.PopMatrix();
    }

    public static void Translate(float v1, float? v2 = null)
    {
        Translate(new Vector2(v1, v2 ?? v1));
    }

    public static void Translate(Vector2 translation)
    {
        Game.EnsureRunning();
        Rlgl.Translatef(translation.X, translation.Y, 0);
    }

    public static void Rotate(float angle, float v1, float? v2 = null)
    {
        Rotate(angle, new Vector2(v1, v2 ?? v1));
    }

    public static void Rotate(float angle, Vector2? position = null)
    {
        Game.EnsureRunning();
        if (position.HasValue)
            Rlgl.Translatef(position.Value.X, position.Value.Y, 0);
        Rlgl.Rotatef(angle, 0, 0, 1);
        if (position.HasValue)
            Rlgl.Translatef(-position.Value.X, -position.Value.Y, 0);
    }

    public static void Scale(float v1, float? v2 = null)
    {
        Scale(new Vector2(v1, v2 ?? v1));
    }

    public static void Scale(Vector2 scale)
    {
        Game.EnsureRunning();
        Rlgl.Scalef(scale.X, scale.Y, 1);
    }

    public static void Transform(Transform transform)
    {
        Transform(ref transform);
    }

    public static void Transform(ref Transform transform)
    {
        Game.EnsureRunning();
        Transform(ref transform, out _, out var scale);
        Scale(scale);
    }

    public void ClearBackground(Color color)
    {
        if (color == Color.Transparent)
            return;
        BeginDraw();
        Raylib.ClearBackground(color.RColor);
        EndDraw();
    }

    #region Rectangle

    public void FillRectangle(float x, float y, float width, float height, Color color, Camera? camera = null)
    {
        FillRectangle(new Vector2(x, y), new Vector2(width, height), color, camera);
    }

    public void FillRectangle(Vector2 position, Vector2 size, Color color, Camera? camera = null)
    {
        if (color == Color.Transparent)
            return;
        BeginDraw(camera);
        Raylib.DrawRectangleRec(new Raylib_cs.Rectangle(position, size), color.RColor);
        EndDraw(camera);
    }

    public void StrokeRectangle(
        float x,
        float y,
        float width,
        float height,
        Color color,
        float strokeWidth = 1,
        Camera? camera = null
    )
    {
        StrokeRectangle(new Vector2(x, y), new Vector2(width, height), color, strokeWidth, camera);
    }

    public void StrokeRectangle(
        Vector2 position,
        Vector2 size,
        Color color,
        float strokeWidth = 1,
        Camera? camera = null
    )
    {
        if (color == Color.Transparent || strokeWidth <= 0)
            return;
        BeginDraw(camera);
        Raylib.DrawRectangleLinesEx(new Raylib_cs.Rectangle(position, size), strokeWidth, color.RColor);
        EndDraw(camera);
    }

    public void FillRoundedRectangle(
        float x,
        float y,
        float width,
        float height,
        Color color,
        float roundness,
        Camera? camera = null
    )
    {
        FillRoundedRectangle(new Vector2(x, y), new Vector2(width, height), color, roundness, camera);
    }

    public void FillRoundedRectangle(
        Vector2 position,
        Vector2 size,
        Color color,
        float roundness,
        Camera? camera = null
    )
    {
        if (color == Color.Transparent)
            return;
        BeginDraw(camera);
        Raylib.DrawRectangleRounded(new Raylib_cs.Rectangle(position, size), roundness, 0, color.RColor);
        EndDraw(camera);
    }

    public void StrokeRoundedRectangle(
        float x,
        float y,
        float width,
        float height,
        Color color,
        float roundness,
        float strokeWidth = 1,
        Camera? camera = null
    )
    {
        StrokeRoundedRectangle(new Vector2(x, y), new Vector2(width, height), color, roundness, strokeWidth, camera);
    }

    public void StrokeRoundedRectangle(
        Vector2 position,
        Vector2 size,
        Color color,
        float roundness,
        float strokeWidth = 1,
        Camera? camera = null
    )
    {
        if (color == Color.Transparent || strokeWidth <= 0)
            return;
        BeginDraw(camera);
        Raylib.DrawRectangleRoundedLinesEx(
            new Raylib_cs.Rectangle(position, size),
            roundness,
            0,
            strokeWidth,
            color.RColor
        );
        EndDraw(camera);
    }

    public void DrawRectangle(Transform transform, Rectangle rectangle)
    {
        DrawRectangle(ref transform, ref rectangle);
    }

    public void DrawRectangle(ref Transform transform, Rectangle rectangle)
    {
        DrawRectangle(ref transform, ref rectangle);
    }

    public void DrawRectangle(Transform transform, ref Rectangle rectangle)
    {
        DrawRectangle(ref transform, ref rectangle);
    }

    public void DrawRectangle(ref Transform transform, ref Rectangle rectangle)
    {
        var camera = rectangle.Camera?.Invoke();
        var fill = rectangle.Fill;
        var stroke = rectangle.Stroke;
        var roundness = rectangle.Roundness;
        var strokeWidth = rectangle.StrokeWidth;
        PushState();
        Transform(ref transform, out var position, out var scale);
        if (roundness > 0)
        {
            FillRoundedRectangle(position, scale, fill, roundness, camera);
            StrokeRoundedRectangle(position, scale, stroke, roundness, strokeWidth, camera);
        }
        else
        {
            FillRectangle(position, scale, fill, camera);
            StrokeRectangle(position, scale, stroke, strokeWidth, camera);
        }

        PopState();
    }

    #endregion

    #region Circle

    public void FillCircle(float x, float y, float radius, Color color, Camera? camera = null)
    {
        FillCircle(new Vector2(x, y), radius, color, camera);
    }

    public void FillCircle(Vector2 center, float radius, Color color, Camera? camera = null)
    {
        if (color == Color.Transparent)
            return;
        BeginDraw(camera);
        Raylib.DrawCircleV(center, radius, color.RColor);
        EndDraw(camera);
    }

    public void StrokeCircle(float x, float y, float radius, Color color, float strokeWidth = 1, Camera? camera = null)
    {
        StrokeCircle(new Vector2(x, y), radius, color, strokeWidth, camera);
    }

    public void StrokeCircle(Vector2 center, float radius, Color color, float strokeWidth = 1, Camera? camera = null)
    {
        if (color == Color.Transparent || strokeWidth <= 0)
            return;
        BeginDraw(camera);
        Raylib.DrawRing(center, radius - strokeWidth, radius + 1, 0, 360, 0, color.RColor);
        EndDraw(camera);
    }

    public void DrawCircle(Transform transform, Circle circle)
    {
        DrawCircle(ref transform, ref circle);
    }

    public void DrawCircle(ref Transform transform, Circle circle)
    {
        DrawCircle(ref transform, ref circle);
    }

    public void DrawCircle(Transform transform, ref Circle circle)
    {
        DrawCircle(ref transform, ref circle);
    }

    public void DrawCircle(ref Transform transform, ref Circle circle)
    {
        var camera = circle.Camera?.Invoke();
        var fill = circle.Fill;
        var stroke = circle.Stroke;
        var strokeWidth = circle.StrokeWidth;
        PushState();
        Transform(ref transform, out var position, out var scale);
        var radius = (scale.X + scale.Y) * 0.25f;
        position += radius;
        FillCircle(position, radius, fill, camera);
        StrokeCircle(position, radius, stroke, strokeWidth, camera);
        PopState();
    }

    #endregion

    #region Text

    public void FillText(
        string text,
        float x,
        float y,
        Color color,
        Font? font = null,
        float? fontSize = null,
        Vector2? spacing = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        FillText(text, new Vector2(x, y), color, font, fontSize, spacing, interpolation, camera);
    }

    public void FillText(
        string text,
        Vector2 position,
        Color color,
        Font? font = null,
        float? fontSize = null,
        Vector2? spacing = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        if (text == "" || color == Color.Transparent)
            return;
        font ??= Game.DefaultFont;
        Raylib.SetTextureFilter(font.Atlas, (TextureFilter)(interpolation ?? Game.DefaultInterpolation));
        BeginDraw(camera);
        var rColor = color.RColor;
        font.HandleText(
            (sourcePosition, sourceSize, destPosition, destSize) =>
            {
                Raylib.DrawTexturePro(
                    font.Atlas,
                    new Raylib_cs.Rectangle(sourcePosition, sourceSize),
                    new Raylib_cs.Rectangle(destPosition + position, destSize),
                    new Vector2(),
                    0,
                    rColor
                );
            },
            text,
            fontSize ?? Game.DefaultFontSize,
            spacing ?? Game.DefaultTextSpacing
        );
        EndDraw(camera);
    }

    public void StrokeText(
        string text,
        float x,
        float y,
        Color color,
        Font? font = null,
        float? fontSize = null,
        float strokeWidth = 1,
        Vector2? spacing = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        StrokeText(text, new Vector2(x, y), color, font, fontSize, strokeWidth, spacing, interpolation, camera);
    }

    public void StrokeText(
        string text,
        Vector2 position,
        Color color,
        Font? font = null,
        float? fontSize = null,
        float strokeWidth = 1,
        Vector2? spacing = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        if (text == "" || color == Color.Transparent || strokeWidth <= 0)
            return;
        font ??= Game.DefaultFont;
        var (atlas, glyphInfos) = font.GetStroke((int)MathF.Round(strokeWidth));
        Raylib.SetTextureFilter(atlas, (TextureFilter)(interpolation ?? Game.DefaultInterpolation));
        BeginDraw(camera);
        var rColor = color.RColor;
        font.HandleText(
            (sourcePosition, sourceSize, destPosition, destSize) =>
            {
                Raylib.DrawTexturePro(
                    atlas,
                    new Raylib_cs.Rectangle(sourcePosition, sourceSize),
                    new Raylib_cs.Rectangle(destPosition + position - strokeWidth * 0.5f, destSize),
                    new Vector2(),
                    0,
                    rColor
                );
            },
            text,
            fontSize ?? Game.DefaultFontSize,
            spacing ?? Game.DefaultTextSpacing,
            glyphInfos
        );
        EndDraw(camera);
    }

    public void DrawText(Transform transform, Text text)
    {
        DrawText(ref transform, ref text);
    }

    public void DrawText(ref Transform transform, Text text)
    {
        DrawText(ref transform, ref text);
    }

    public void DrawText(Transform transform, ref Text text)
    {
        DrawText(ref transform, ref text);
    }

    public void DrawText(ref Transform transform, ref Text text)
    {
        var camera = text.Camera?.Invoke();
        var value = text.Value;
        var fill = text.Fill;
        var stroke = text.Stroke;
        var font = text.Font;
        var fontSize = text.FontSize;
        var strokeWidth = text.StrokeWidth;
        var spacing = text.Spacing;
        var interpolation = text.Interpolation;
        var scale = transform.Scale;
        PushState();
        fontSize *= (MathF.Abs(scale.X) + MathF.Abs(scale.Y)) / 2;
        transform.Scale = text.Font.MeasureText(value, fontSize, spacing);
        Transform(ref transform, out var position, out _);
        transform.Scale = scale;
        FillText(value, position, fill, font, fontSize, spacing, interpolation, camera);
        StrokeText(value, position, stroke, font, fontSize, strokeWidth, spacing, interpolation, camera);
        PopState();
    }

    #endregion

    private void BeginDraw(Camera? camera = null)
    {
        if (camera.HasValue)
        {
            PushState();
            var cam = camera.Value;
            var camera2D = new Camera2D(cam.Offset, cam.Target, cam.Rotation, cam.Zoom);
            Rlgl.MultMatrixf(Raylib.GetCameraMatrix2D(camera2D));
        }

        if (CurrentBuffer == Buffer)
            return;
        CurrentBuffer = Buffer;
        Raylib.EndTextureMode();
        Raylib.BeginTextureMode(Buffer.RenderTexture2D);
    }

    private static void EndDraw(Camera? camera = null)
    {
        if (camera.HasValue)
            PopState();
    }

    private static void Transform(ref Transform transform, out Vector2 position, out Vector2 scale)
    {
        position = transform.Position;
        scale = transform.Scale.Abs();
        var pivotPoint = transform.PivotPoint;
        var rotation = transform.Rotation;
        var positionOffset = -(scale * 0.5f);
        var rotationOffset = position + pivotPoint;
        Rotate(rotation, rotationOffset);
        Translate(positionOffset);
    }
}
