using System.Numerics;
using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;
using Transform = Vigilance.Math.Transform;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.Drawing;

public sealed class Graphics
{
    internal static WritableTexture? CurrentBuffer;
    private readonly WritableTexture _buffer;
    private readonly Stack<Matrix4x4> _matrixStack = new();
    private Matrix4x4 _matrix = Matrix4x4.Identity;

    internal Graphics(WritableTexture buffer)
    {
        _buffer = buffer;
    }

    #region Matrix

    public Matrix4x4 GetMatrix()
    {
        return _matrixStack.Count == 0 ? _matrix : _matrixStack.Peek();
    }

    public void PushMatrix()
    {
        _matrixStack.Push(GetMatrix());
    }

    public void PopMatrix()
    {
        if (_matrixStack.Count != 0)
            _matrixStack.Pop();
    }

    public void MultiplyMatrix(Matrix4x4 matrix)
    {
        if (_matrixStack.Count == 0)
        {
            _matrix *= matrix;
            return;
        }

        var top = _matrixStack.Pop();
        _matrixStack.Push(matrix * top);
    }

    public void Translate(float v1, float? v2 = null)
    {
        MultiplyMatrix(Matrix4x4.CreateTranslation(v1, v2 ?? v1, 0));
    }

    public void Translate(Vector2 translation)
    {
        MultiplyMatrix(Matrix4x4.CreateTranslation(translation.X, translation.Y, 0));
    }

    public void Rotate(float angle, float v1, float? v2 = null)
    {
        Rotate(angle, new Vector2(v1, v2 ?? v1));
    }

    public void Rotate(float angle, Vector2? position = null)
    {
        if (position.HasValue)
            MultiplyMatrix(Matrix4x4.CreateTranslation(position.Value.X, position.Value.Y, 0));
        MultiplyMatrix(Matrix4x4.CreateRotationZ(MathF.PI * angle / 180f));
        if (position.HasValue)
            MultiplyMatrix(Matrix4x4.CreateTranslation(-position.Value.X, -position.Value.Y, 0));
    }

    public void Scale(float v1, float? v2 = null)
    {
        Scale(new Vector2(v1, v2 ?? v1));
    }

    public void Scale(Vector2 scale)
    {
        MultiplyMatrix(Matrix4x4.CreateScale(scale.X, scale.Y, 1f));
    }

    public void Transform(Transform transform)
    {
        var position = transform.Position;
        var scale = transform.Scale;
        var pivotPoint = transform.PivotPoint;
        var rotation = transform.Rotation;
        Scale(scale);
        Translate(position);
        Rotate(rotation, pivotPoint);
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
        EndDrawing();
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
        EndDrawing();
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
        EndDrawing();
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
        EndDrawing();
    }

    public void DrawRectangle(Transform transform, Rectangle rectangle)
    {
        var camera = rectangle.Camera?.Invoke();
        var fill = rectangle.Fill;
        var stroke = rectangle.Stroke;
        var roundness = rectangle.Roundness;
        var strokeWidth = rectangle.StrokeWidth;
        var position = transform.Position;
        var scale = transform.Scale.Abs();
        PushMatrix();
        Transform(transform, true);
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

        PopMatrix();
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
        EndDrawing();
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
        EndDrawing();
    }

    public void DrawCircle(Transform transform, Circle circle)
    {
        var camera = circle.Camera?.Invoke();
        var fill = circle.Fill;
        var stroke = circle.Stroke;
        var strokeWidth = circle.StrokeWidth;
        var position = transform.Position;
        var scale = transform.Scale;
        PushMatrix();
        Transform(transform, false);
        var radius = (MathF.Abs(scale.X) + MathF.Abs(scale.Y)) * 0.25f;
        FillCircle(position, radius, fill, camera);
        StrokeCircle(position, radius, stroke, strokeWidth, camera);
        PopMatrix();
    }

    #endregion

    #region Triangle

    public void FillTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Color color, Camera? camera = null)
    {
        FillCustomPolygon([v1, v2, v3], color, camera);
    }

    public void StrokeTriangle(
        Vector2 v1,
        Vector2 v2,
        Vector2 v3,
        Color color,
        float strokeWidth = 1,
        Camera? camera = null
    )
    {
        StrokeCustomPolygon([v1, v2, v3], color, strokeWidth, camera);
    }

    public void DrawTriangle(Transform transform, Triangle triangle)
    {
        DrawCustomPolygon(
            transform,
            new CustomPolygon
            {
                Points = [triangle.V1, triangle.V2, triangle.V3],
                Fill = triangle.Fill,
                Stroke = triangle.Stroke,
                StrokeWidth = triangle.StrokeWidth,
                Camera = triangle.Camera,
            }
        );
    }

    #endregion

    #region Polygon

    public void FillRegularPolygon(float x, float y, int sides, float radius, Color color, Camera? camera = null)
    {
        FillRegularPolygon(new Vector2(x, y), sides, radius, color, camera);
    }

    public void FillRegularPolygon(Vector2 center, int sides, float radius, Color color, Camera? camera = null)
    {
        if (color == Color.Transparent)
            return;
        BeginDrawing(camera);
        Raylib.DrawPoly(center, sides, radius, 0, color.RColor);
        EndDrawing();
    }

    public void StrokeRegularPolygon(
        float x,
        float y,
        int sides,
        float radius,
        Color color,
        float strokeWidth = 1,
        Camera? camera = null
    )
    {
        StrokeRegularPolygon(new Vector2(x, y), sides, radius, color, strokeWidth, camera);
    }

    public void StrokeRegularPolygon(
        Vector2 center,
        int sides,
        float radius,
        Color color,
        float strokeWidth = 1,
        Camera? camera = null
    )
    {
        if (color == Color.Transparent || strokeWidth <= 0)
            return;
        BeginDrawing(camera);
        Raylib.DrawPolyLinesEx(center, sides, radius, 0, strokeWidth, color.RColor);
        EndDrawing();
    }

    public void DrawRegularPolygon(Transform transform, RegularPolygon polygon)
    {
        var camera = polygon.Camera?.Invoke();
        var sides = polygon.Sides;
        var fill = polygon.Fill;
        var stroke = polygon.Stroke;
        var strokeWidth = polygon.StrokeWidth;
        var position = transform.Position;
        var scale = transform.Scale;
        PushMatrix();
        Transform(transform, false);
        var radius = (MathF.Abs(scale.X) + MathF.Abs(scale.Y)) * 0.25f;
        FillRegularPolygon(position, sides, radius, fill, camera);
        StrokeRegularPolygon(position, sides, radius, stroke, strokeWidth, camera);
        PopMatrix();
    }

    public unsafe void FillCustomPolygon(IReadOnlyList<Vector2> points, Color color, Camera? camera = null)
    {
        if (color == Color.Transparent || points.Count < 3)
            return;
        BeginDrawing(camera);
        fixed (Vector2* pointsBuffer = points as Vector2[] ?? points.ToArray())
        {
            Raylib.DrawTriangleFan((System.Numerics.Vector2*)pointsBuffer, points.Count, color.RColor);
        }

        EndDrawing();
    }

    public void StrokeCustomPolygon(
        IReadOnlyList<Vector2> points,
        Color color,
        float strokeWidth = 1,
        Camera? camera = null
    )
    {
        if (color == Color.Transparent || strokeWidth <= 0 || points.Count < 3)
            return;
        BeginDrawing(camera);
        for (var i = 0; i < points.Count; i++)
        {
            var start = points[i];
            var end = points[(i + 1) % points.Count];
            Raylib.DrawLineEx(start, end, strokeWidth, color.RColor);
            Raylib.DrawCircleV(start, strokeWidth * 0.5f, color.RColor);
        }

        EndDrawing();
    }

    public void DrawCustomPolygon(Transform transform, CustomPolygon polygon)
    {
        var camera = polygon.Camera?.Invoke();
        var position = transform.Position;
        var scale = transform.Scale;
        var points = Coordinates.Scale(polygon.Points, scale, position);
        var fill = polygon.Fill;
        var stroke = polygon.Stroke;
        var strokeWidth = polygon.StrokeWidth;
        PushMatrix();
        Transform(transform, false);
        FillCustomPolygon(points, fill, camera);
        StrokeCustomPolygon(points, stroke, strokeWidth, camera);
        PopMatrix();
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
        EndDrawing();
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
        var changeLineWidth = !Precision.AreEqual(lineWidth, strokeWidth);
        if (changeLineWidth)
        {
            Rlgl.DrawRenderBatchActive();
            Rlgl.SetLineWidth(strokeWidth);
        }

        BeginDrawing(camera);
        Raylib.DrawRingLines(center, innerRadius, outerRadius, startAngle, endAngle, 0, color.RColor);
        EndDrawing();
        if (!changeLineWidth)
            return;
        Rlgl.DrawRenderBatchActive();
        Rlgl.SetLineWidth(lineWidth);
    }

    public void DrawRing(Transform transform, Ring ring)
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
        PushMatrix();
        Transform(transform, false);
        FillRing(position, innerRadius, outerRadius, startAngle, endAngle, fill, camera);
        StrokeRing(position, innerRadius, outerRadius, startAngle, endAngle, stroke, strokeWidth, camera);
        PopMatrix();
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
        EndDrawing();
    }

    public void DrawLine(Transform transform, Line line)
    {
        var camera = line.Camera?.Invoke();
        var position = transform.Position;
        var start = line.Start + position;
        var end = line.End + position;
        var color = line.Color;
        var thickness = line.Thickness;
        var scale = (MathF.Abs(transform.Scale.X) + MathF.Abs(transform.Scale.Y)) * 0.5f;
        PushMatrix();
        Transform(transform, false);
        DrawLine(start, end, color, thickness * scale, camera);
        PopMatrix();
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
        EndDrawing();
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
        EndDrawing();
    }

    public void DrawText(Transform transform, Text text)
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
        PushMatrix();
        fontSize *= (MathF.Abs(scale.X) + MathF.Abs(scale.Y)) * 0.5f;
        transform.Scale = text.Font.MeasureText(value, fontSize, spacing);
        Transform(transform, true);
        FillText(value, position, fill, font, fontSize, spacing, interpolation, camera);
        StrokeText(value, position, stroke, font, fontSize, strokeWidth, spacing, interpolation, camera);
        PopMatrix();
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
        EndDrawing();
    }

    public void DrawSprite(Transform transform, Sprite sprite)
    {
        var camera = sprite.Camera?.Invoke();
        var texture = sprite.Texture;
        var interpolation = sprite.Interpolation;
        var tint = sprite.Tint;
        var flipX = sprite.FlipX;
        var flipY = sprite.FlipY;
        var position = transform.Position;
        var scale = transform.Scale.Abs();
        var source = sprite.Source ?? new Box(Vector2.Zero, new Vector2(texture.Width, texture.Height));
        if (flipX)
            source.Width = -source.Width;
        if (flipY)
            source.Height = -source.Height;
        PushMatrix();
        Transform(transform, true);
        DrawTexture(texture, source, new Box(position, scale), tint, interpolation, camera);
        PopMatrix();
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

    #region Internal

    private unsafe void BeginDrawing(Camera? camera = null)
    {
        var matrix = GetMatrix();
        Rlgl.PushMatrix();
        if (camera != null)
            Rlgl.MultMatrixf(Raylib.GetCameraMatrix2D(camera.RCamera));
        Rlgl.MultMatrixf(&matrix.M11);
        if (CurrentBuffer == _buffer)
            return;
        CurrentBuffer = _buffer;
        Raylib.EndTextureMode();
        Raylib.BeginTextureMode(_buffer.RenderTexture2D);
    }

    private static void EndDrawing()
    {
        Rlgl.PopMatrix();
    }

    private void Transform(Transform transform, bool translate)
    {
        var position = transform.Position;
        var scale = transform.Scale.Abs();
        var pivotPoint = transform.PivotPoint;
        var rotation = transform.Rotation;
        var positionOffset = -(scale * 0.5f);
        var rotationOffset = position + pivotPoint;
        Rotate(rotation, rotationOffset);
        if (translate)
            Translate(positionOffset);
    }

    #endregion
}
