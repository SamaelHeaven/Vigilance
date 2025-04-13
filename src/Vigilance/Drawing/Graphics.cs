using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

public readonly struct Graphics
{
    internal static WritableTexture? CurrentBuffer;
    private readonly WritableTexture _buffer;

    internal Graphics(WritableTexture buffer)
    {
        _buffer = buffer;
    }

    #region Transform

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

    public static void Transform(Transform transform, bool translate = true, bool rotate = true, bool scale = true)
    {
        Game.EnsureRunning();
        var position = transform.Position;
        var scaling = transform.Scale.Abs();
        var pivotPoint = transform.PivotPoint;
        var rotation = transform.Rotation;
        var positionOffset = -(scaling * 0.5f);
        var rotationOffset = position + pivotPoint;
        if (scale)
            Scale(scaling);
        if (rotate)
            Rotate(rotation, rotationOffset);
        if (translate)
            Translate(positionOffset);
    }

    #endregion

    #region Rectangle

    public void FillRectangle(float x, float y, float width, float height, Color color, Camera? camera = null)
    {
        FillRectangle(new Vector2(x, y), new Vector2(width, height), color, camera);
    }

    public void FillRectangle(Box box, Color color, Camera? camera = null)
    {
        FillRectangle(box.Position, box.Size, color, camera);
    }

    public void FillRectangle(Vector2 position, Vector2 size, Color color, Camera? camera = null)
    {
        if (color == Color.Transparent)
            return;
        BeginDrawing(camera);
        Raylib.DrawRectangleRec(new Raylib_cs.Rectangle(position, size), color.RColor);
        EndDrawing(camera);
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

    public void StrokeRectangle(Box box, Color color, float strokeWidth = 1, Camera? camera = null)
    {
        StrokeRectangle(box.Position, box.Size, color, strokeWidth, camera);
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
        BeginDrawing(camera);
        Raylib.DrawRectangleLinesEx(new Raylib_cs.Rectangle(position, size), strokeWidth, color.RColor);
        EndDrawing(camera);
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

    public void FillRoundedRectangle(Box box, Color color, float roundness, Camera? camera = null)
    {
        FillRoundedRectangle(box.Position, box.Size, color, roundness, camera);
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
        BeginDrawing(camera);
        Raylib.DrawRectangleRounded(new Raylib_cs.Rectangle(position, size), roundness, 0, color.RColor);
        EndDrawing(camera);
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
        Box box,
        Color color,
        float roundness,
        float strokeWidth = 1,
        Camera? camera = null
    )
    {
        StrokeRoundedRectangle(box.Position, box.Size, color, roundness, strokeWidth, camera);
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
        BeginDrawing(camera);
        Raylib.DrawRectangleRoundedLinesEx(
            new Raylib_cs.Rectangle(position, size),
            roundness,
            0,
            strokeWidth,
            color.RColor
        );
        EndDrawing(camera);
    }

    public void DrawRectangle(Transform transform, Rectangle rectangle)
    {
        DrawRectangle(transform, ref rectangle);
    }

    public void DrawRectangle(Transform transform, ref readonly Rectangle rectangle)
    {
        var camera = rectangle.Camera?.Invoke();
        var fill = rectangle.Fill;
        var stroke = rectangle.Stroke;
        var roundness = rectangle.Roundness;
        var strokeWidth = rectangle.StrokeWidth;
        var position = transform.Position;
        var scale = transform.Scale;
        PushState();
        Transform(transform, scale: false);
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
        BeginDrawing(camera);
        Raylib.DrawCircleV(center, radius, color.RColor);
        EndDrawing(camera);
    }

    public void StrokeCircle(float x, float y, float radius, Color color, float strokeWidth = 1, Camera? camera = null)
    {
        StrokeCircle(new Vector2(x, y), radius, color, strokeWidth, camera);
    }

    public void StrokeCircle(Vector2 center, float radius, Color color, float strokeWidth = 1, Camera? camera = null)
    {
        if (color == Color.Transparent || strokeWidth <= 0)
            return;
        BeginDrawing(camera);
        Raylib.DrawRing(center, radius - strokeWidth, radius + 1, 0, 360, 0, color.RColor);
        EndDrawing(camera);
    }

    public void DrawCircle(Transform transform, Circle circle)
    {
        DrawCircle(transform, ref circle);
    }

    public void DrawCircle(Transform transform, ref readonly Circle circle)
    {
        var camera = circle.Camera?.Invoke();
        var fill = circle.Fill;
        var stroke = circle.Stroke;
        var strokeWidth = circle.StrokeWidth;
        var position = transform.Position;
        var scale = transform.Scale;
        PushState();
        Transform(transform, false, scale: false);
        var radius = (MathF.Abs(scale.X) + MathF.Abs(scale.Y)) * 0.25f;
        FillCircle(position, radius, fill, camera);
        StrokeCircle(position, radius, stroke, strokeWidth, camera);
        PopState();
    }

    #endregion

    #region Ring

    public void FillRing(
        float x,
        float y,
        float innerRadius,
        float outerRadius,
        float startAngle,
        float endAngle,
        Color color,
        Camera? camera = null
    )
    {
        FillRing(new Vector2(x, y), innerRadius, outerRadius, startAngle, endAngle, color, camera);
    }

    public void FillRing(
        Vector2 center,
        float innerRadius,
        float outerRadius,
        float startAngle,
        float endAngle,
        Color color,
        Camera? camera = null
    )
    {
        if (color == Color.Transparent)
            return;
        BeginDrawing(camera);
        Raylib.DrawRing(center, innerRadius, outerRadius, startAngle, endAngle, 0, color.RColor);
        EndDrawing(camera);
    }

    public void StrokeRing(
        float x,
        float y,
        float innerRadius,
        float outerRadius,
        float startAngle,
        float endAngle,
        Color color,
        float strokeWidth = 1,
        Camera? camera = null
    )
    {
        StrokeRing(new Vector2(x, y), innerRadius, outerRadius, startAngle, endAngle, color, strokeWidth, camera);
    }

    public void StrokeRing(
        Vector2 center,
        float innerRadius,
        float outerRadius,
        float startAngle,
        float endAngle,
        Color color,
        float strokeWidth = 1,
        Camera? camera = null
    )
    {
        if (color == Color.Transparent || strokeWidth <= 0)
            return;
        var lineWidth = Rlgl.GetLineWidth();
        var changeLineWidth = !Precision.AreEqual(strokeWidth, lineWidth);
        if (changeLineWidth)
        {
            Rlgl.DrawRenderBatchActive();
            Rlgl.SetLineWidth(strokeWidth);
        }

        BeginDrawing(camera);
        Raylib.DrawRingLines(center, innerRadius, outerRadius, startAngle, endAngle, 0, color.RColor);
        EndDrawing(camera);
        if (!changeLineWidth)
            return;
        Rlgl.DrawRenderBatchActive();
        Rlgl.SetLineWidth(lineWidth);
    }

    public void DrawRing(Transform transform, Ring ring)
    {
        DrawRing(transform, ref ring);
    }

    public void DrawRing(Transform transform, ref readonly Ring ring)
    {
        var camera = ring.Camera?.Invoke();
        var startAngle = ring.StartAngle;
        var endAngle = ring.EndAngle;
        var fill = ring.Fill;
        var stroke = ring.Stroke;
        var strokeWidth = ring.StrokeWidth;
        var position = transform.Position;
        var scale = (MathF.Abs(transform.Scale.X) + MathF.Abs(transform.Scale.Y)) * 0.5f;
        var innerRadius = ring.InnerRadius * scale;
        var outerRadius = ring.OuterRadius * scale;
        PushState();
        Transform(transform, false, scale: false);
        FillRing(position, innerRadius, outerRadius, startAngle, endAngle, fill, camera);
        StrokeRing(position, innerRadius, outerRadius, startAngle, endAngle, stroke, strokeWidth, camera);
        PopState();
    }

    #endregion

    #region Line

    public void DrawLine(
        float startX,
        float startY,
        float endX,
        float endY,
        Color color,
        float thickness = 1,
        Camera? camera = null
    )
    {
        DrawLine(new Vector2(startX, startY), new Vector2(endX, endY), color, thickness, camera);
    }

    public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1, Camera? camera = null)
    {
        if (color == Color.Transparent)
            return;
        BeginDrawing(camera);
        Raylib.DrawLineEx(start, end, thickness, color.RColor);
        EndDrawing(camera);
    }

    public void DrawLine(Transform transform, Line line)
    {
        DrawLine(transform, ref line);
    }

    public void DrawLine(Transform transform, ref readonly Line line)
    {
        var camera = line.Camera?.Invoke();
        var position = transform.Position;
        var start = line.Start + position;
        var end = line.End + position;
        var color = line.Color;
        var thickness = line.Thickness;
        var scale = (MathF.Abs(transform.Scale.X) + MathF.Abs(transform.Scale.Y)) * 0.5f;
        PushState();
        Transform(transform, false, scale: false);
        DrawLine(start, end, color, thickness * scale, camera);
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
        BeginDrawing(camera);
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
        EndDrawing(camera);
    }

    public void StrokeText(
        string text,
        float x,
        float y,
        Color color,
        Font? font = null,
        float? fontSize = null,
        float strokeWidth = 4,
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
        float strokeWidth = 4,
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
        BeginDrawing(camera);
        var rColor = color.RColor;
        font.HandleText(
            (sourcePosition, sourceSize, destPosition, destSize) =>
            {
                Raylib.DrawTexturePro(
                    atlas,
                    new Raylib_cs.Rectangle(sourcePosition, sourceSize),
                    new Raylib_cs.Rectangle(destPosition + position, destSize),
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
        EndDrawing(camera);
    }

    public void DrawText(Transform transform, Text text)
    {
        DrawText(transform, ref text);
    }

    public void DrawText(Transform transform, ref readonly Text text)
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
        var position = transform.Position;
        var scale = transform.Scale;
        PushState();
        fontSize *= (MathF.Abs(scale.X) + MathF.Abs(scale.Y)) * 0.5f;
        transform.Scale = text.Font.MeasureText(value, fontSize, spacing);
        Transform(transform, scale: false);
        FillText(value, position, fill, font, fontSize, spacing, interpolation, camera);
        StrokeText(value, position, stroke, font, fontSize, strokeWidth, spacing, interpolation, camera);
        PopState();
    }

    #endregion

    #region Texture

    public void DrawTexture(
        Texture texture,
        float x,
        float y,
        Color? tint = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        DrawTexture(texture, new Vector2(x, y), null, tint, interpolation, camera);
    }

    public void DrawTexture(
        Texture texture,
        float x,
        float y,
        float width,
        float height,
        Color? tint = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        DrawTexture(texture, new Vector2(x, y), new Vector2(width, height), tint, interpolation, camera);
    }

    public void DrawTexture(
        Texture texture,
        Box box,
        Color? tint = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        DrawTexture(texture, box.Position, box.Size, tint, interpolation, camera);
    }

    public void DrawTexture(
        Texture texture,
        Vector2 position,
        Vector2? size = null,
        Color? tint = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        DrawTexture(
            texture,
            new Box(Vector2.Zero, texture.Size),
            new Box(position, size ?? texture.Size),
            tint,
            interpolation,
            camera
        );
    }

    public void DrawTexture(
        Texture texture,
        Box source,
        Box dest,
        Color? tint = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        Raylib.SetTextureFilter(texture.Texture2D, (TextureFilter)(interpolation ?? Game.DefaultInterpolation));
        BeginDrawing(camera);
        var rSource = new Raylib_cs.Rectangle(
            source.X,
            source.Y,
            source.Width,
            texture.Writable ? -source.Height : source.Height
        );
        var rDest = new Raylib_cs.Rectangle(dest.Position, dest.Size);
        Raylib.DrawTexturePro(texture.Texture2D, rSource, rDest, Vector2.Zero, 0, (tint ?? Color.White).RColor);
        EndDrawing(camera);
    }

    public void DrawSprite(Transform transform, Sprite sprite)
    {
        DrawSprite(transform, ref sprite);
    }

    public void DrawSprite(Transform transform, ref readonly Sprite sprite)
    {
        var camera = sprite.Camera?.Invoke();
        var texture = sprite.Texture;
        var interpolation = sprite.Interpolation;
        var tint = sprite.Tint;
        var flipX = sprite.FlipX;
        var flipY = sprite.FlipY;
        var position = transform.Position;
        var scale = transform.Scale;
        PushState();
        Transform(transform, scale: false);
        DrawTexture(
            texture,
            new Box(
                Vector2.Zero,
                new Vector2(flipX ? -texture.Width : texture.Width, flipY ? -texture.Height : texture.Height)
            ),
            new Box(position, scale),
            tint,
            interpolation,
            camera
        );
        PopState();
    }

    #endregion

    #region Misc

    public void ClearBackground(Color color)
    {
        if (color == Color.Transparent)
            return;
        BeginDrawing();
        Raylib.ClearBackground(color.RColor);
        EndDrawing();
    }

    public void DrawPixel(float x, float y, Color color)
    {
        DrawPixel(new Vector2(x, y), color);
    }

    public void DrawPixel(Vector2 position, Color color)
    {
        BeginDrawing();
        Raylib.DrawPixelV(position, color.RColor);
        EndDrawing();
    }

    #endregion

    private void BeginDrawing(Camera? camera = null)
    {
        if (camera.HasValue)
        {
            PushState();
            var cam = camera.Value;
            var camera2D = new Camera2D(cam.Offset, cam.Target, cam.Rotation, cam.Zoom);
            Rlgl.MultMatrixf(Raylib.GetCameraMatrix2D(camera2D));
        }

        if (CurrentBuffer == _buffer)
            return;
        CurrentBuffer = _buffer;
        Raylib.EndTextureMode();
        Raylib.BeginTextureMode(_buffer.RenderTexture2D);
    }

    private static void EndDrawing(Camera? camera = null)
    {
        if (camera.HasValue)
            PopState();
    }
}
