using System.Numerics;
using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;
using Transform = Vigilance.Math.Transform;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.Drawing;

public sealed unsafe class Graphics
{
    private static WritableTexture? _currentBuffer = null;
    private static Box? _currentClip = null;
    private readonly WritableTexture? _buffer;
    private readonly Stack<Matrix3x2> _matrixStack = new();
    private Box? _clip = null;
    private bool _drawing = false;
    private Matrix3x2 _matrix = Matrix3x2.Identity;

    internal Graphics(WritableTexture? buffer)
    {
        _buffer = buffer;
    }

    #region Entity

    public void DrawEntity(Entity entity)
    {
        if (entity.TryGet(out Rectangle rectangle))
            DrawRectangle(entity.WorldTransform, rectangle);
        if (entity.TryGet(out Circle circle))
            DrawCircle(entity.WorldTransform, circle);
        if (entity.TryGet(out Triangle triangle))
            DrawTriangle(entity.WorldTransform, triangle);
        if (entity.TryGet(out RegularPolygon regularPolygon))
            DrawRegularPolygon(entity.WorldTransform, regularPolygon);
        if (entity.TryGet(out CustomPolygon customPolygon))
            DrawCustomPolygon(entity.WorldTransform, customPolygon);
        if (entity.TryGet(out Ring ring))
            DrawRing(entity.WorldTransform, ring);
        if (entity.TryGet(out Line line))
            DrawLine(entity.WorldTransform, line);
        if (entity.TryGet(out Text text))
            DrawText(entity.WorldTransform, text);
        if (entity.TryGet(out Sprite sprite))
            DrawSprite(entity.WorldTransform, sprite);
        if (entity.TryGet(out Grid grid))
            DrawGrid(entity.WorldTransform, grid);
    }

    #endregion

    #region Matrix

    public Matrix3x2 GetMatrix()
    {
        return _matrixStack.Count == 0 ? _matrix : _matrixStack.Peek();
    }

    public void LoadIdentity()
    {
        _matrixStack.Clear();
        _matrix = Matrix3x2.Identity;
    }

    public void PushMatrix()
    {
        _matrixStack.Push(GetMatrix());
    }

    public void PushMatrix(Matrix3x2 matrix)
    {
        _matrixStack.Push(matrix);
    }

    public Matrix3x2 PopMatrix()
    {
        return _matrixStack.Count != 0 ? _matrixStack.Pop() : _matrix;
    }

    public void MultiplyMatrix(Matrix3x2 matrix)
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
        MultiplyMatrix(Matrix3x2.CreateTranslation(v1, v2 ?? v1));
    }

    public void Translate(Vector2 translation)
    {
        MultiplyMatrix(Matrix3x2.CreateTranslation(translation.X, translation.Y));
    }

    public void Rotate(float angle, float v1, float? v2 = null)
    {
        Rotate(angle, new Vector2(v1, v2 ?? v1));
    }

    public void Rotate(float angle, Vector2? position = null)
    {
        if (position.HasValue)
            MultiplyMatrix(Matrix3x2.CreateTranslation(position.Value.X, position.Value.Y));
        MultiplyMatrix(Matrix3x2.CreateRotation(angle.DegToRad()));
        if (position.HasValue)
            MultiplyMatrix(Matrix3x2.CreateTranslation(-position.Value.X, -position.Value.Y));
    }

    public void Scale(float v1, float? v2 = null)
    {
        Scale(new Vector2(v1, v2 ?? v1));
    }

    public void Scale(Vector2 scale)
    {
        MultiplyMatrix(Matrix3x2.CreateScale(scale.X, scale.Y));
    }

    public void Skew(float v1, float? v2 = null)
    {
        Skew(new Vector2(v1, v2 ?? v1));
    }

    public void Skew(Vector2 skew)
    {
        MultiplyMatrix(Matrix3x2.CreateSkew(skew.X, skew.Y));
    }

    public void Transform(Transform transform)
    {
        var rotation = transform.Rotation;
        var pivotPoint = transform.PivotPoint;
        var position = transform.Position;
        var scale = transform.Scale.Abs();
        Rotate(rotation, pivotPoint);
        Translate(position);
        Scale(scale);
    }

    public void Pivot(Transform transform, bool translate)
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

    #region Clip

    public void SetClip(float x, float y, float width, float height)
    {
        _clip = new Box(x, y, width, height);
    }

    public void SetClip(Vector2 position, Vector2 size)
    {
        _clip = new Box(position, size);
    }

    public void SetClip(Box? clip)
    {
        _clip = clip;
    }

    public Box? GetClip()
    {
        return _clip;
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
        var strokeWidth = MathF.Max(0, rectangle.StrokeWidth);
        var position = transform.Position;
        var scale = transform.Scale.Abs();
        PushMatrix();
        Pivot(transform, true);
        if (roundness > 0)
        {
            position += strokeWidth;
            scale -= strokeWidth * 2;
            FillRoundedRectangle(position, scale, fill, roundness, camera);
            StrokeRoundedRectangle(position, scale, stroke, roundness, strokeWidth, camera);
        }
        else
        {
            FillRectangle(position + strokeWidth, scale - strokeWidth * 2, fill, camera);
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
        Pivot(transform, false);
        var radius = (scale.X.Abs() + scale.Y.Abs()) * 0.25f;
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
        Pivot(transform, false);
        var radius = (scale.X.Abs() + scale.Y.Abs()) * 0.25f;
        FillRegularPolygon(position, sides, radius, fill, camera);
        StrokeRegularPolygon(position, sides, radius, stroke, strokeWidth, camera);
        PopMatrix();
    }

    public void FillCustomPolygon(IEnumerable<Vector2> points, Color color, Camera? camera = null)
    {
        var span = points.AsSpan();
        if (color == Color.Transparent || span.Length < 3)
            return;
        BeginDrawing(camera);
        fixed (Vector2* pointsBuffer = span)
        {
            Raylib.DrawTriangleFan((System.Numerics.Vector2*)pointsBuffer, span.Length, color.RColor);
        }

        EndDrawing();
    }

    public void StrokeCustomPolygon(
        IEnumerable<Vector2> points,
        Color color,
        float strokeWidth = 1,
        Camera? camera = null
    )
    {
        var span = points.AsSpan();
        if (color == Color.Transparent || strokeWidth <= 0 || span.Length < 3)
            return;
        BeginDrawing(camera);
        for (var i = 0; i < span.Length; i++)
        {
            var start = span[i];
            var end = span[(i + 1) % span.Length];
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
        Pivot(transform, false);
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
            DrawCurrentBuffer();
            Rlgl.SetLineWidth(strokeWidth);
        }

        BeginDrawing(camera);
        Raylib.DrawRingLines(center, innerRadius, outerRadius, startAngle, endAngle, 0, color.RColor);
        EndDrawing();
        if (!changeLineWidth)
            return;
        DrawCurrentBuffer();
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
        var scale = (transform.Scale.X.Abs() + transform.Scale.Y.Abs()) * 0.5f;
        var innerRadius = ring.InnerRadius * scale;
        var outerRadius = ring.OuterRadius * scale;
        PushMatrix();
        Pivot(transform, false);
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
        float thick = 1,
        Camera? camera = null
    )
    {
        DrawLine(new Vector2(startX, startY), new Vector2(endX, endY), color, thick, camera);
    }

    public void DrawLine(Vector2 start, Vector2 end, Color color, float thick = 1, Camera? camera = null)
    {
        if (color == Color.Transparent || thick <= 0)
            return;
        BeginDrawing(camera);
        Raylib.DrawLineEx(start, end, thick, color.RColor);
        EndDrawing();
    }

    public void DrawLine(Transform transform, Line line)
    {
        var camera = line.Camera?.Invoke();
        var position = transform.Position;
        var start = line.Start + position;
        var end = line.End + position;
        var color = line.Color;
        var thick = line.Thick;
        var scale = (transform.Scale.X.Abs() + transform.Scale.Y.Abs()) * 0.5f;
        PushMatrix();
        Pivot(transform, false);
        DrawLine(start, end, color, thick * scale, camera);
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
        Raylib.SetTextureFilter(font.Atlas.Texture2D, (TextureFilter)(interpolation ?? Interpolation.Nearest));
        BeginDrawing(camera);
        var rColor = color.RColor;
        foreach (var (source, dest) in font.GetTextBounds(text, fontSize, spacing))
            Raylib.DrawTexturePro(
                font.Atlas.Texture2D,
                new Raylib_cs.Rectangle(source.Position, source.Size),
                new Raylib_cs.Rectangle(dest.Position + position, dest.Size),
                new Vector2(),
                0,
                rColor
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
        var (atlas, glyphInfos) = font.GetStroke((int)strokeWidth.Round());
        Raylib.SetTextureFilter(atlas.Texture2D, (TextureFilter)(interpolation ?? Interpolation.Nearest));
        BeginDrawing(camera);
        var rColor = color.RColor;
        foreach (var (source, dest) in font.GetTextBounds(text, fontSize, spacing, glyphInfos))
            Raylib.DrawTexturePro(
                atlas.Texture2D,
                new Raylib_cs.Rectangle(source.Position, source.Size),
                new Raylib_cs.Rectangle(dest.Position + position, dest.Size),
                new Vector2(),
                0,
                rColor
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
        fontSize *= (scale.X.Abs() + scale.Y.Abs()) * 0.5f;
        transform.Scale = text.Font.MeasureText(value, fontSize, spacing);
        Pivot(transform, true);
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
        Raylib.SetTextureFilter(texture.Texture2D, (TextureFilter)(interpolation ?? Interpolation.Nearest));
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
        Pivot(transform, true);
        DrawTexture(texture, source, new Box(position, scale), tint, interpolation, camera);
        PopMatrix();
    }

    #endregion

    #region Grid

    public void DrawGrid(
        float x,
        float y,
        float width,
        float height,
        float cellSize,
        Color color,
        float thick = 1,
        Camera? camera = null
    )
    {
        DrawGrid(new Vector2(x, y), new Vector2(width, height), cellSize, color, thick, camera);
    }

    public void DrawGrid(Box box, float cellSize, Color color, float thick = 1, Camera? camera = null)
    {
        DrawGrid(box.Position, box.Size, cellSize, color, thick, camera);
    }

    public void DrawGrid(
        Vector2 position,
        Vector2 size,
        float cellSize,
        Color color,
        float thick = 1,
        Camera? camera = null
    )
    {
        if (color == Color.Transparent || thick <= 0)
            return;
        BeginDrawing(camera);
        for (var x = position.X; x <= position.X + size.X; x += cellSize)
            Raylib.DrawLineEx(new Vector2(x, position.Y), new Vector2(x, position.Y + size.Y), thick, color.RColor);
        for (var y = position.Y; y <= position.Y + size.Y; y += cellSize)
            Raylib.DrawLineEx(new Vector2(position.X, y), new Vector2(position.X + size.X, y), thick, color.RColor);
        EndDrawing();
    }

    public void DrawGrid(Transform transform, Grid grid)
    {
        var camera = grid.Camera?.Invoke();
        var color = grid.Color;
        var cellSize = grid.CellSize;
        var thick = grid.Thick;
        var position = transform.Position;
        var scale = transform.Scale.Abs();
        PushMatrix();
        Pivot(transform, true);
        DrawGrid(position, scale, cellSize, color, thick, camera);
        PopMatrix();
    }

    #endregion

    #region Spline

    public void DrawSplineLinear(IEnumerable<Vector2> points, Color color, float thick = 1, Camera? camera = null)
    {
        var span = points.AsSpan();
        if (color == Color.Transparent || span.Length < 2 || thick <= 0)
            return;
        BeginDrawing(camera);
        fixed (Vector2* pointsBuffer = span)
        {
            Raylib.DrawSplineLinear((System.Numerics.Vector2*)pointsBuffer, span.Length, thick, color.RColor);
        }

        EndDrawing();
    }

    public void DrawSplineBasis(IEnumerable<Vector2> points, Color color, float thick = 1, Camera? camera = null)
    {
        var span = points.AsSpan();
        if (color == Color.Transparent || span.Length < 4 || thick <= 0)
            return;
        BeginDrawing(camera);
        fixed (Vector2* pointsBuffer = span)
        {
            Raylib.DrawSplineBasis((System.Numerics.Vector2*)pointsBuffer, span.Length, thick, color.RColor);
        }

        EndDrawing();
    }

    public void DrawSplineCatmullRom(IEnumerable<Vector2> points, Color color, float thick = 1, Camera? camera = null)
    {
        var span = points.AsSpan();
        if (color == Color.Transparent || span.Length < 4 || thick <= 0)
            return;
        BeginDrawing(camera);
        fixed (Vector2* pointsBuffer = span)
        {
            Raylib.DrawSplineCatmullRom((System.Numerics.Vector2*)pointsBuffer, span.Length, thick, color.RColor);
        }

        EndDrawing();
    }

    public void DrawSplineBezierQuadratic(
        IEnumerable<Vector2> points,
        Color color,
        float thick = 1,
        Camera? camera = null
    )
    {
        var span = points.AsSpan();
        if (color == Color.Transparent || span.Length < 3 || thick <= 0)
            return;
        BeginDrawing(camera);
        fixed (Vector2* pointsBuffer = span)
        {
            Raylib.DrawSplineBezierQuadratic((System.Numerics.Vector2*)pointsBuffer, span.Length, thick, color.RColor);
        }

        EndDrawing();
    }

    public void DrawSplineBezierCubic(IEnumerable<Vector2> points, Color color, float thick = 1, Camera? camera = null)
    {
        var span = points.AsSpan();
        if (color == Color.Transparent || span.Length < 4 || thick <= 0)
            return;
        BeginDrawing(camera);
        fixed (Vector2* pointsBuffer = span)
        {
            Raylib.DrawSplineBezierCubic((System.Numerics.Vector2*)pointsBuffer, span.Length, thick, color.RColor);
        }

        EndDrawing();
    }

    public void DrawSplineSegmentLinear(Vector2 p1, Vector2 p2, Color color, float thick = 1, Camera? camera = null)
    {
        if (color == Color.Transparent || thick <= 0)
            return;
        BeginDrawing(camera);
        Raylib.DrawSplineSegmentLinear(p1, p2, thick, color.RColor);
        EndDrawing();
    }

    public void DrawSplineSegmentBasis(
        Vector2 p1,
        Vector2 p2,
        Vector2 p3,
        Vector2 p4,
        Color color,
        float thick = 1,
        Camera? camera = null
    )
    {
        if (color == Color.Transparent || thick <= 0)
            return;
        BeginDrawing(camera);
        Raylib.DrawSplineSegmentBasis(p1, p2, p3, p4, thick, color.RColor);
        EndDrawing();
    }

    public void DrawSplineSegmentCatmullRom(
        Vector2 p1,
        Vector2 p2,
        Vector2 p3,
        Vector2 p4,
        Color color,
        float thick = 1,
        Camera? camera = null
    )
    {
        if (color == Color.Transparent || thick <= 0)
            return;
        BeginDrawing(camera);
        Raylib.DrawSplineSegmentCatmullRom(p1, p2, p3, p4, thick, color.RColor);
        EndDrawing();
    }

    public void DrawSplineSegmentCatmullRom(
        Vector2 p1,
        Vector2 p2,
        Vector2 p3,
        Color color,
        float thick = 1,
        Camera? camera = null
    )
    {
        if (color == Color.Transparent || thick <= 0)
            return;
        BeginDrawing(camera);
        Raylib.DrawSplineSegmentBezierQuadratic(p1, p2, p3, thick, color.RColor);
        EndDrawing();
    }

    public void DrawSplineSegmentBezierCubic(
        Vector2 p1,
        Vector2 p2,
        Vector2 p3,
        Vector2 p4,
        Color color,
        float thick = 1,
        Camera? camera = null
    )
    {
        if (color == Color.Transparent || thick <= 0)
            return;
        BeginDrawing(camera);
        Raylib.DrawSplineSegmentBezierCubic(p1, p2, p3, p4, thick, color.RColor);
        EndDrawing();
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

    #region Drawing

    public void BeginDrawing(Camera? camera = null)
    {
        if (_drawing)
            throw new InvalidOperationException("Cannot begin drawing while already drawing.");
        _drawing = true;
        var offset = Renderer.Offset;
        var scale = Renderer.Scale;
        if (_currentBuffer != _buffer)
        {
            if (_currentBuffer is null)
                DrawCurrentBuffer();
            else
                Raylib.EndTextureMode();
            _currentBuffer = _buffer;
            if (_buffer is not null)
                Raylib.BeginTextureMode(_buffer.RenderTexture2D);
        }

        var clip = _clip;
        if (clip is not null)
            clip = new Box(clip.Value.Position * scale + offset, clip.Value.Size * scale);
        if (!Precision.AreEqual(_currentClip, clip))
        {
            if (_currentClip.HasValue)
                Raylib.EndScissorMode();
            _currentClip = clip;
            if (clip.HasValue)
            {
                Raylib.BeginScissorMode(
                    (int)clip.Value.X.Round(),
                    (int)clip.Value.Y.Round(),
                    (int)clip.Value.Width.Round(),
                    (int)clip.Value.Height.Round()
                );
            }
        }

        var matrix3X2 = GetMatrix();
        Rlgl.PushMatrix();
        Rlgl.Translatef(offset.X, offset.Y, 0);
        Rlgl.Scalef(scale.X, scale.Y, 1);
        if (camera is not null)
            matrix3X2 *= camera.Matrix;
        var matrix4X4 = new Matrix4x4(
            matrix3X2.M11,
            matrix3X2.M12,
            0,
            0,
            matrix3X2.M21,
            matrix3X2.M22,
            0,
            0,
            0,
            0,
            1,
            0,
            matrix3X2.M31,
            matrix3X2.M32,
            0,
            1
        );
        Rlgl.MultMatrixf(&matrix4X4.M11);
    }

    public void EndDrawing()
    {
        if (!_drawing)
            throw new InvalidOperationException($"{nameof(BeginDrawing)} must be called before {nameof(EndDrawing)}.");
        _drawing = false;
        Rlgl.PopMatrix();
    }

    #endregion

    #region Internal

    internal static bool IsBufferCurrent(WritableTexture? buffer)
    {
        return _currentBuffer == buffer;
    }
    
    internal static void Reset()
    {
        if (_currentBuffer is not null)
        {
            Raylib.EndTextureMode();
            _currentBuffer = null;
        }

        if (!_currentClip.HasValue)
            return;
        Raylib.EndScissorMode();
        _currentClip = null;
    }

    internal static void DrawCurrentBuffer()
    {
        Rlgl.DrawRenderBatchActive();
    }

    #endregion
}
