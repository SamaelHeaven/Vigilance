using System.Numerics;
using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vigilance.Math;
using Transform = Vigilance.Math.Transform;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.Drawing;

public sealed unsafe class Graphics
{
    private const float PixelOffset = 0.375f;
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
        if (entity.TryGet(out RectangleGradient rectangleGradient))
            DrawRectangleGradient(entity.WorldTransform, rectangleGradient);
        if (entity.TryGet(out Circle circle))
            DrawCircle(entity.WorldTransform, circle);
        if (entity.TryGet(out CircleGradient circleGradient))
            DrawCircleGradient(entity.WorldTransform, circleGradient);
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

    #region Bounds

    public Box GetBounds(Camera? camera = null, float offset = 0)
    {
        return GetBounds(GetMatrix(camera), offset);
    }

    public Box GetBounds(Matrix3x2 matrix, float offset = 0)
    {
        if (!Precision.AreEqual(offset, 0))
        {
            var scaleX = MathF.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
            var scaleY = MathF.Sqrt(matrix.M21 * matrix.M21 + matrix.M22 * matrix.M22);
            offset *= scaleX.Max(scaleY);
        }

        var clip = GetClip();
        return new Box(
            (clip?.Position ?? Vector2.Zero) - offset,
            (clip?.Size ?? _buffer?.Size ?? Display.Size) + offset * 2
        );
    }

    public bool IsBoxInBounds(float x, float y, float width, float height, Camera? camera, float offset = 0)
    {
        return IsBoxInBounds(new Box(x, y, width, height), camera, offset);
    }

    public bool IsBoxInBounds(Vector2 position, Vector2 size, Camera? camera, float offset = 0)
    {
        return IsBoxInBounds(new Box(position, size), camera, offset);
    }

    public bool IsBoxInBounds(Box box, Camera? camera, float offset = 0)
    {
        var matrix = GetMatrix(camera);
        return Collision.CheckPolygonsSpan(box.Transform(matrix), (Quad)GetBounds(matrix, offset));
    }

    public bool IsPolygonInBounds(IEnumerable<Vector2> points, Camera? camera, float offset = 0)
    {
        return IsPolygonInBoundsSpan(points.AsSpan(), camera, offset);
    }

    public bool IsPolygonInBoundsSpan(ReadOnlySpan<Vector2> points, Camera? camera, float offset = 0)
    {
        var matrix = GetMatrix(camera);
        if (points.Length > 128)
        {
            var transformedPoints = new Vector2[points.Length];
            for (var i = 0; i < points.Length; i++)
                transformedPoints[i] = points[i].Transform(matrix);
            points = transformedPoints;
        }
        else
        {
            var transformedPoints = stackalloc Vector2[points.Length];
            for (var i = 0; i < points.Length; i++)
                transformedPoints[i] = points[i].Transform(matrix);
            points = new ReadOnlySpan<Vector2>(transformedPoints, points.Length);
        }

        return Collision.CheckPolygonsSpan(points, (Quad)GetBounds(matrix, offset));
    }

    #endregion

    #region Matrix

    public Matrix3x2 GetMatrix()
    {
        return _matrixStack.Count == 0 ? _matrix : _matrixStack.Peek();
    }

    public Matrix3x2 GetMatrix(Camera? camera)
    {
        return camera is not null ? GetMatrix() * camera.Matrix : GetMatrix();
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

    public void FillRectangle(float x, float y, float width, float height, Color? color = null, Camera? camera = null)
    {
        FillRectangle(new Vector2(x, y), new Vector2(width, height), color, camera);
    }

    public void FillRectangle(Box box, Color? color = null, Camera? camera = null)
    {
        FillRectangle(box.Position, box.Size, color, camera);
    }

    public void FillRectangle(Vector2 position, Vector2 size, Color? color = null, Camera? camera = null)
    {
        var colorValue = color ?? Drawing.DefaultFill;
        if (colorValue == Color.Transparent || !IsBoxInBounds(position, size, camera))
            return;
        BeginDrawing(camera);
        Raylib.DrawRectangleRec(new Raylib_cs.BleedingEdge.Rectangle(position, size), colorValue.RColor);
        EndDrawing();
    }

    public void FillRectangleGradient(
        float x,
        float y,
        float width,
        float height,
        Color? topLeftColor = null,
        Color? bottomLeftColor = null,
        Color? bottomRightColor = null,
        Color? topRightColor = null,
        Camera? camera = null
    )
    {
        FillRectangleGradient(
            new Vector2(x, y),
            new Vector2(width, height),
            topLeftColor,
            bottomLeftColor,
            bottomRightColor,
            topRightColor,
            camera
        );
    }

    public void FillRectangleGradient(
        Box box,
        Color? topLeftColor = null,
        Color? bottomLeftColor = null,
        Color? bottomRightColor = null,
        Color? topRightColor = null,
        Camera? camera = null
    )
    {
        FillRectangleGradient(
            box.Position,
            box.Size,
            topLeftColor,
            bottomLeftColor,
            bottomRightColor,
            topRightColor,
            camera
        );
    }

    public void FillRectangleGradient(
        Vector2 position,
        Vector2 size,
        Color? topLeftColor = null,
        Color? bottomLeftColor = null,
        Color? bottomRightColor = null,
        Color? topRightColor = null,
        Camera? camera = null
    )
    {
        var topLeftColorValue = topLeftColor ?? Drawing.DefaultFill;
        var bottomLeftColorValue = bottomLeftColor ?? Drawing.DefaultFill;
        var bottomRightColorValue = bottomRightColor ?? Drawing.DefaultFill;
        var topRightColorValue = topRightColor ?? Drawing.DefaultFill;
        if (
            (
                topLeftColorValue == Color.Transparent
                && bottomLeftColorValue == Color.Transparent
                && bottomRightColorValue == Color.Transparent
                && topRightColorValue == Color.Transparent
            ) || !IsBoxInBounds(position, size, camera)
        )
            return;
        BeginDrawing(camera);
        Raylib.DrawRectangleGradientEx(
            new Raylib_cs.BleedingEdge.Rectangle(position, size),
            topLeftColorValue.RColor,
            bottomLeftColorValue.RColor,
            bottomRightColorValue.RColor,
            topRightColorValue.RColor
        );
        EndDrawing();
    }

    public void StrokeRectangle(
        float x,
        float y,
        float width,
        float height,
        Color? color = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        StrokeRectangle(new Vector2(x, y), new Vector2(width, height), color, strokeWidth, camera);
    }

    public void StrokeRectangle(Box box, Color? color = null, float? strokeWidth = null, Camera? camera = null)
    {
        StrokeRectangle(box.Position, box.Size, color, strokeWidth, camera);
    }

    public void StrokeRectangle(
        Vector2 position,
        Vector2 size,
        Color? color = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultStroke;
        var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
        if (colorValue == Color.Transparent || strokeWidthValue <= 0 || !IsBoxInBounds(position, size, camera))
            return;
        BeginDrawing(camera);
        Raylib.DrawRectangleLinesEx(
            new Raylib_cs.BleedingEdge.Rectangle(position, size),
            strokeWidthValue,
            colorValue.RColor
        );
        EndDrawing();
    }

    public void FillRoundedRectangle(
        float x,
        float y,
        float width,
        float height,
        Color? color = null,
        float? roundness = null,
        Camera? camera = null
    )
    {
        FillRoundedRectangle(new Vector2(x, y), new Vector2(width, height), color, roundness, camera);
    }

    public void FillRoundedRectangle(Box box, Color? color = null, float? roundness = null, Camera? camera = null)
    {
        FillRoundedRectangle(box.Position, box.Size, color, roundness, camera);
    }

    public void FillRoundedRectangle(
        Vector2 position,
        Vector2 size,
        Color? color = null,
        float? roundness = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill;
        var roundnessValue = roundness ?? Drawing.DefaultRoundness.Or(0.1f);
        if (colorValue == Color.Transparent || roundnessValue <= 0 || !IsBoxInBounds(position, size, camera))
            return;
        BeginDrawing(camera);
        Raylib.DrawRectangleRounded(
            new Raylib_cs.BleedingEdge.Rectangle(position, size),
            roundnessValue,
            0,
            colorValue.RColor
        );
        EndDrawing();
    }

    public void StrokeRoundedRectangle(
        float x,
        float y,
        float width,
        float height,
        Color? color = null,
        float? roundness = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        StrokeRoundedRectangle(new Vector2(x, y), new Vector2(width, height), color, roundness, strokeWidth, camera);
    }

    public void StrokeRoundedRectangle(
        Box box,
        Color? color = null,
        float? roundness = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        StrokeRoundedRectangle(box.Position, box.Size, color, roundness, strokeWidth, camera);
    }

    public void StrokeRoundedRectangle(
        Vector2 position,
        Vector2 size,
        Color? color = null,
        float? roundness = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultStroke;
        var roundnessValue = roundness ?? Drawing.DefaultRoundness.Or(0.1f);
        var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
        if (
            colorValue == Color.Transparent
            || roundnessValue <= 0
            || strokeWidthValue <= 0
            || !IsBoxInBounds(position, size, camera, strokeWidthValue)
        )
            return;
        BeginDrawing(camera);
        Raylib.DrawRectangleRoundedLinesEx(
            new Raylib_cs.BleedingEdge.Rectangle(position, size),
            roundnessValue,
            0,
            strokeWidthValue,
            colorValue.RColor
        );
        EndDrawing();
    }

    public void DrawRectangle(float x, float y, float width, float height, Rectangle rectangle)
    {
        DrawRectangle(new Vector2(x, y), new Vector2(width, height), rectangle);
    }

    public void DrawRectangle(Vector2 position, Vector2 size, Rectangle rectangle)
    {
        DrawRectangle(new Transform(position + size * 0.5f, size), rectangle);
    }

    public void DrawRectangle(Box box, Rectangle rectangle)
    {
        DrawRectangle(box.Position, box.Size, rectangle);
    }

    public void DrawRectangle(Transform transform, Rectangle rectangle)
    {
        var camera = rectangle.Camera.Get();
        var fill = rectangle.Fill;
        var stroke = rectangle.Stroke;
        var roundness = rectangle.Roundness.Abs();
        var position = transform.Position;
        var scale = transform.Scale.Abs();
        var strokeWidth = rectangle.StrokeWidth.Clamp(
            scale.X.Min(scale.Y) * 0.5f - (roundness > 0 ? PixelOffset : 0),
            0
        );
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

    public void DrawRectangleGradient(float x, float y, float width, float height, RectangleGradient rectangle)
    {
        DrawRectangleGradient(new Vector2(x, y), new Vector2(width, height), rectangle);
    }

    public void DrawRectangleGradient(Vector2 position, Vector2 size, RectangleGradient rectangle)
    {
        DrawRectangleGradient(new Transform(position + size * 0.5f, size), rectangle);
    }

    public void DrawRectangleGradient(Box box, RectangleGradient rectangle)
    {
        DrawRectangleGradient(box.Position, box.Size, rectangle);
    }

    public void DrawRectangleGradient(Transform transform, RectangleGradient rectangle)
    {
        var camera = rectangle.Camera.Get();
        var topLeftFill = rectangle.TopLeftFill;
        var bottomLeftFill = rectangle.BottomLeftFill;
        var bottomRightFill = rectangle.BottomRightFill;
        var topRightFill = rectangle.TopRightFill;
        var stroke = rectangle.Stroke;
        var position = transform.Position;
        var scale = transform.Scale.Abs();
        var strokeWidth = rectangle.StrokeWidth.Clamp(scale.X.Min(scale.Y) * 0.5f, 0);
        PushMatrix();
        Pivot(transform, true);
        FillRectangleGradient(
            position + strokeWidth,
            scale - strokeWidth * 2,
            topLeftFill,
            bottomLeftFill,
            bottomRightFill,
            topRightFill,
            camera
        );
        StrokeRectangle(position, scale, stroke, strokeWidth, camera);
        PopMatrix();
    }

    #endregion

    #region Circle

    public void FillCircle(float x, float y, float radius, Color? color = null, Camera? camera = null)
    {
        FillCircle(new Vector2(x, y), radius, color, camera);
    }

    public void FillCircle(Vector2 center, float radius, Color? color = null, Camera? camera = null)
    {
        var colorValue = color ?? Drawing.DefaultFill;
        if (colorValue == Color.Transparent || !IsBoxInBounds(center - radius, new Vector2(radius * 2), camera))
            return;
        BeginDrawing(camera);
        Raylib.DrawCircleV(center, radius, colorValue.RColor);
        EndDrawing();
    }

    public void FillCircleGradient(
        Vector2 center,
        float radius,
        Color? innerColor = null,
        Color? outerColor = null,
        Camera? camera = null
    )
    {
        var innerColorValue = innerColor ?? Drawing.DefaultFill;
        var outerColorValue = outerColor ?? Drawing.DefaultFill;
        if (
            (innerColorValue == Color.Transparent && outerColorValue == Color.Transparent)
            || !IsBoxInBounds(center - radius, new Vector2(radius * 2), camera)
        )
            return;
        BeginDrawing(camera);
        Rlgl.Begin(RlglEnum.Triangles);
        for (var i = 0; i < 360; i += 10)
        {
            Rlgl.Color4ub(innerColorValue.R, innerColorValue.G, innerColorValue.B, innerColorValue.A);
            Rlgl.Vertex2f(center.X, center.Y);
            Rlgl.Color4ub(outerColorValue.R, outerColorValue.G, outerColorValue.B, outerColorValue.A);
            Rlgl.Vertex2f(
                center.X + MathF.Cos((i + 10f).DegToRad()) * radius,
                center.Y + MathF.Sin((i + 10f).DegToRad()) * radius
            );
            Rlgl.Color4ub(outerColorValue.R, outerColorValue.G, outerColorValue.B, outerColorValue.A);
            Rlgl.Vertex2f(
                center.X + MathF.Cos(((float)i).DegToRad()) * radius,
                center.Y + MathF.Sin(((float)i).DegToRad()) * radius
            );
        }

        Rlgl.End();
        EndDrawing();
    }

    public void StrokeCircle(
        float x,
        float y,
        float radius,
        Color? color = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        StrokeCircle(new Vector2(x, y), radius, color, strokeWidth, camera);
    }

    public void StrokeCircle(
        Vector2 center,
        float radius,
        Color? color = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultStroke;
        var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
        if (
            colorValue == Color.Transparent
            || strokeWidthValue <= 0
            || !IsBoxInBounds(center - radius, new Vector2(radius * 2), camera)
        )
            return;
        BeginDrawing(camera);
        Raylib.DrawRing(center, radius - strokeWidthValue, radius + 1, 0, 360, 0, colorValue.RColor);
        EndDrawing();
    }

    public void DrawCircle(Transform transform, Circle circle)
    {
        var camera = circle.Camera.Get();
        var fill = circle.Fill;
        var stroke = circle.Stroke;
        var strokeWidth = circle.StrokeWidth;
        var position = transform.Position;
        var scale = transform.Scale;
        var radius = scale.X.Abs().Min(scale.Y.Abs()) * 0.5f;
        PushMatrix();
        Pivot(transform, false);
        FillCircle(position, radius, fill, camera);
        StrokeCircle(position, radius, stroke, strokeWidth, camera);
        PopMatrix();
    }

    public void DrawCircleGradient(Transform transform, CircleGradient circle)
    {
        var camera = circle.Camera.Get();
        var innerFill = circle.InnerFill;
        var outerFill = circle.OuterFill;
        var stroke = circle.Stroke;
        var strokeWidth = circle.StrokeWidth;
        var position = transform.Position;
        var scale = transform.Scale;
        var radius = scale.X.Abs().Min(scale.Y.Abs()) * 0.5f;
        PushMatrix();
        Pivot(transform, false);
        FillCircleGradient(position, radius, innerFill, outerFill, camera);
        StrokeCircle(position, radius, stroke, strokeWidth, camera);
        PopMatrix();
    }

    #endregion

    #region Triangle

    public void FillTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Color? color = null, Camera? camera = null)
    {
        var points = stackalloc Vector2[3];
        points[0] = v1;
        points[1] = v2;
        points[2] = v3;
        var span = new ReadOnlySpan<Vector2>(points, 3);
        FillCustomPolygonSpan(span, color, camera);
    }

    public void StrokeTriangle(
        Vector2 v1,
        Vector2 v2,
        Vector2 v3,
        Color? color = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        var points = stackalloc Vector2[3];
        points[0] = v1;
        points[1] = v2;
        points[2] = v3;
        var span = new ReadOnlySpan<Vector2>(points, 3);
        StrokeCustomPolygonSpan(span, color, strokeWidth, camera);
    }

    public void DrawTriangle(Transform transform, Triangle triangle)
    {
        var camera = triangle.Camera.Get();
        var position = transform.Position;
        var scale = transform.Scale;
        var scaledPoints = Coordinates.Scale(triangle.Points, scale, position);
        var fill = triangle.Fill;
        var stroke = triangle.Stroke;
        var strokeWidth = triangle.StrokeWidth;
        PushMatrix();
        Pivot(transform, false);
        var points = stackalloc Vector2[3];
        var i = 0;
        foreach (var point in scaledPoints)
            points[i++] = point;
        var span = new ReadOnlySpan<Vector2>(points, 3);
        FillCustomPolygonSpan(span, fill, camera);
        StrokeCustomPolygonSpan(span, stroke, strokeWidth, camera);
        PopMatrix();
    }

    #endregion

    #region Polygon

    public void FillRegularPolygon(
        float x,
        float y,
        int sides,
        float radius,
        Color? color = null,
        Camera? camera = null
    )
    {
        FillRegularPolygon(new Vector2(x, y), sides, radius, color, camera);
    }

    public void FillRegularPolygon(Vector2 center, int sides, float radius, Color? color = null, Camera? camera = null)
    {
        var colorValue = color ?? Drawing.DefaultFill;
        if (color == Color.Transparent || sides < 3 || !IsBoxInBounds(center - radius, new Vector2(radius * 2), camera))
            return;
        BeginDrawing(camera);
        Raylib.DrawPoly(center, sides, radius, 0, colorValue.RColor);
        EndDrawing();
    }

    public void StrokeRegularPolygon(
        float x,
        float y,
        int sides,
        float radius,
        Color? color = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        StrokeRegularPolygon(new Vector2(x, y), sides, radius, color, strokeWidth, camera);
    }

    public void StrokeRegularPolygon(
        Vector2 center,
        int sides,
        float radius,
        Color? color = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultStroke;
        var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
        if (
            colorValue == Color.Transparent
            || sides < 3
            || strokeWidthValue <= 0
            || !IsBoxInBounds(center - radius, new Vector2(radius * 2), camera)
        )
            return;
        BeginDrawing(camera);
        Raylib.DrawPolyLinesEx(center, sides, radius, 0, radius.Min(strokeWidthValue), colorValue.RColor);
        EndDrawing();
    }

    public void DrawRegularPolygon(Transform transform, RegularPolygon polygon)
    {
        var camera = polygon.Camera.Get();
        var sides = polygon.Sides;
        var fill = polygon.Fill;
        var stroke = polygon.Stroke;
        var strokeWidth = polygon.StrokeWidth;
        var position = transform.Position;
        var scale = transform.Scale;
        PushMatrix();
        Pivot(transform, false);
        var radius = scale.X.Abs().Min(scale.Y.Abs()) * 0.5f;
        FillRegularPolygon(position, sides, radius, fill, camera);
        StrokeRegularPolygon(position, sides, radius, stroke, strokeWidth, camera);
        PopMatrix();
    }

    public void FillCustomPolygon(IEnumerable<Vector2> points, Color? color = null, Camera? camera = null)
    {
        FillCustomPolygonSpan(points.AsSpan(), color, camera);
    }

    public void FillCustomPolygonSpan(ReadOnlySpan<Vector2> points, Color? color = null, Camera? camera = null)
    {
        var colorValue = color ?? Drawing.DefaultFill;
        if (colorValue == Color.Transparent || points.Length < 3 || !IsPolygonInBoundsSpan(points, camera))
            return;
        BeginDrawing(camera);
        fixed (Vector2* pointsBuffer = points)
        {
            Raylib.DrawTriangleFan((System.Numerics.Vector2*)pointsBuffer, points.Length, colorValue.RColor);
        }

        EndDrawing();
    }

    public void StrokeCustomPolygon(
        IEnumerable<Vector2> points,
        Color? color = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        StrokeCustomPolygonSpan(points.AsSpan(), color, strokeWidth, camera);
    }

    public void StrokeCustomPolygonSpan(
        ReadOnlySpan<Vector2> points,
        Color? color = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultStroke;
        var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
        if (
            colorValue == Color.Transparent
            || strokeWidthValue <= 0
            || points.Length < 3
            || !IsPolygonInBoundsSpan(points, camera, strokeWidthValue * 0.5f)
        )
            return;
        BeginDrawing(camera);
        for (var i = 0; i < points.Length; i++)
        {
            var start = points[i];
            var end = points[(i + 1) % points.Length];
            Raylib.DrawLineEx(start, end, strokeWidthValue, colorValue.RColor);
            Raylib.DrawCircleV(start, strokeWidthValue * 0.5f, colorValue.RColor);
        }

        EndDrawing();
    }

    public void DrawCustomPolygon(Transform transform, CustomPolygon polygon)
    {
        var camera = polygon.Camera.Get();
        var position = transform.Position;
        var scale = transform.Scale;
        var scaledPoints = Coordinates.Scale(polygon.Points, scale, position);
        var fill = polygon.Fill;
        var stroke = polygon.Stroke;
        var strokeWidth = polygon.StrokeWidth;
        PushMatrix();
        Pivot(transform, false);
        ReadOnlySpan<Vector2> span;
        if (polygon.Points.Count > 128)
        {
            span = scaledPoints.AsSpan();
        }
        else
        {
            var points = stackalloc Vector2[polygon.Points.Count];
            var i = 0;
            foreach (var point in scaledPoints)
                points[i++] = point;
            span = new ReadOnlySpan<Vector2>(points, polygon.Points.Count);
        }

        FillCustomPolygonSpan(span, fill, camera);
        StrokeCustomPolygonSpan(span, stroke, strokeWidth, camera);
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
        Color? color = null,
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
        Color? color = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill;
        var radius = innerRadius.Max(outerRadius);
        if (colorValue == Color.Transparent || !IsBoxInBounds(center - radius, new Vector2(radius * 2), camera))
            return;
        BeginDrawing(camera);
        Raylib.DrawRing(center, innerRadius, outerRadius, startAngle, endAngle, 0, colorValue.RColor);
        EndDrawing();
    }

    public void StrokeRing(
        float x,
        float y,
        float innerRadius,
        float outerRadius,
        float startAngle,
        float endAngle,
        Color? color = null,
        float? strokeWidth = null,
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
        Color? color = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultStroke;
        var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
        var radius = innerRadius.Max(outerRadius);
        if (
            colorValue == Color.Transparent
            || strokeWidthValue <= 0
            || !IsBoxInBounds(center - radius, new Vector2(radius * 2), camera, strokeWidthValue * 0.5f)
        )
            return;
        var lineWidth = Rlgl.GetLineWidth();
        var changeLineWidth = !Precision.AreEqual(lineWidth, strokeWidthValue);
        if (changeLineWidth)
        {
            DrawCurrentBuffer();
            Rlgl.SetLineWidth(strokeWidthValue);
        }

        BeginDrawing(camera);
        Raylib.DrawRingLines(center, innerRadius, outerRadius, startAngle, endAngle, 0, colorValue.RColor);
        EndDrawing();
        if (!changeLineWidth)
            return;
        DrawCurrentBuffer();
        Rlgl.SetLineWidth(lineWidth);
    }

    public void DrawRing(Transform transform, Ring ring)
    {
        var camera = ring.Camera.Get();
        var startAngle = ring.StartAngle;
        var endAngle = ring.EndAngle;
        var fill = ring.Fill;
        var stroke = ring.Stroke;
        var strokeWidth = ring.StrokeWidth;
        var position = transform.Position;
        var scale = transform.Scale.X.Abs().Min(transform.Scale.Y.Abs());
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
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        DrawLine(new Vector2(startX, startY), new Vector2(endX, endY), color, thick, camera);
    }

    public void DrawLine(Vector2 start, Vector2 end, Color? color = null, float? thick = null, Camera? camera = null)
    {
        var colorValue = color ?? Drawing.DefaultFill;
        var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
        if (
            colorValue == Color.Transparent
            || thickValue <= 0
            || !IsPolygonInBoundsSpan(new Quad(start, start, end, end), camera, thickValue * 0.5f)
        )
            return;
        BeginDrawing(camera);
        Raylib.DrawLineEx(start, end, thickValue, colorValue.RColor);
        EndDrawing();
    }

    public void DrawLine(Transform transform, Line line)
    {
        var camera = line.Camera.Get();
        var position = transform.Position;
        var start = line.Start + position;
        var end = line.End + position;
        var color = line.Color;
        var thick = line.Thick;
        var scale = transform.Scale.X.Abs().Min(transform.Scale.Y.Abs());
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
        Color? color = null,
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
        Color? color = null,
        Font? font = null,
        float? fontSize = null,
        Vector2? spacing = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill;
        if (text == "" || colorValue == Color.Transparent)
            return;
        font ??= Font.Default;
        foreach (var (source, dest) in font.GetTextBounds(text, fontSize, spacing))
            DrawTexture(
                font.Atlas,
                source,
                new Box(dest.Position + position, dest.Size),
                colorValue,
                interpolation,
                camera
            );
    }

    public void StrokeText(
        string text,
        float x,
        float y,
        Color? color = null,
        Font? font = null,
        float? fontSize = null,
        float? strokeWidth = null,
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
        Color? color = null,
        Font? font = null,
        float? fontSize = null,
        float? strokeWidth = null,
        Vector2? spacing = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultStroke;
        var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(4);
        if (text == "" || colorValue == Color.Transparent || strokeWidthValue <= 0)
            return;
        font ??= Font.Default;
        var (atlas, glyphInfos) = font.GetStroke((int)strokeWidthValue.Round());
        foreach (var (source, dest) in font.GetTextBounds(text, fontSize, spacing, glyphInfos))
            DrawTexture(atlas, source, new Box(dest.Position + position, dest.Size), colorValue, interpolation, camera);
    }

    public void DrawText(float x, float y, Text text)
    {
        DrawText(new Vector2(x, y), text);
    }

    public void DrawText(Vector2 position, Text text)
    {
        DrawText(new Transform(position + text.Size * 0.5f), text);
    }

    public void DrawText(Transform transform, Text text)
    {
        var camera = text.Camera.Get();
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
        fontSize *= (scale.X.Abs() + scale.Y.Abs()) * 0.5f;
        transform.Scale = text.Size;
        PushMatrix();
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
        if (!IsBoxInBounds(dest, camera))
            return;
        Raylib.SetTextureFilter(texture.Texture2D, (TextureFilter)(interpolation ?? Interpolation.Nearest));
        BeginDrawing(camera);
        var rSource = new Raylib_cs.BleedingEdge.Rectangle(
            source.X,
            source.Y,
            source.Width,
            texture.Writable ? -source.Height : source.Height
        );
        var rDest = new Raylib_cs.BleedingEdge.Rectangle(dest.Position, dest.Size);
        Raylib.DrawTexturePro(texture.Texture2D, rSource, rDest, Vector2.Zero, 0, (tint ?? Color.White).RColor);
        EndDrawing();
    }

    public void DrawSprite(float x, float y, float width, float height, Sprite sprite)
    {
        DrawSprite(new Vector2(x, y), new Vector2(width, height), sprite);
    }

    public void DrawSprite(Vector2 position, Vector2 size, Sprite sprite)
    {
        DrawSprite(new Transform(position + size * 0.5f, size), sprite);
    }

    public void DrawSprite(Box box, Sprite sprite)
    {
        DrawSprite(box.Position, box.Size, sprite);
    }

    public void DrawSprite(Transform transform, Sprite sprite)
    {
        var camera = sprite.Camera.Get();
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
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        DrawGrid(new Vector2(x, y), new Vector2(width, height), cellSize, color, thick, camera);
    }

    public void DrawGrid(Box box, float cellSize, Color? color = null, float? thick = null, Camera? camera = null)
    {
        DrawGrid(box.Position, box.Size, cellSize, color, thick, camera);
    }

    public void DrawGrid(
        Vector2 position,
        Vector2 size,
        float cellSize,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill;
        var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
        if (colorValue == Color.Transparent || thickValue <= 0)
            return;
        cellSize = cellSize.Max(1);
        for (var x = position.X; x <= position.X + size.X; x += cellSize)
            DrawLine(new Vector2(x, position.Y), new Vector2(x, position.Y + size.Y), colorValue, thickValue, camera);
        for (var y = position.Y; y <= position.Y + size.Y; y += cellSize)
            DrawLine(new Vector2(position.X, y), new Vector2(position.X + size.X, y), colorValue, thickValue, camera);
    }

    public void DrawGrid(Transform transform, Grid grid)
    {
        var camera = grid.Camera.Get();
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

    public void DrawGrid(float x, float y, float width, float height, Grid grid)
    {
        DrawGrid(new Vector2(x, y), new Vector2(width, height), grid);
    }

    public void DrawGrid(Vector2 position, Vector2 size, Grid grid)
    {
        DrawGrid(new Transform(position + size * 0.5f, size), grid);
    }

    public void DrawGrid(Box box, Grid grid)
    {
        DrawGrid(box.Position, box.Size, grid);
    }

    #endregion

    #region Spline

    public void DrawSplineLinear(
        IEnumerable<Vector2> points,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        DrawSplineLinearSpan(points.AsSpan(), color, thick, camera);
    }

    public void DrawSplineLinearSpan(
        ReadOnlySpan<Vector2> points,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill;
        var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
        if (colorValue == Color.Transparent || points.Length < 2 || thickValue <= 0)
            return;
        BeginDrawing(camera);
        fixed (Vector2* pointsBuffer = points)
        {
            Raylib.DrawSplineLinear(
                (System.Numerics.Vector2*)pointsBuffer,
                points.Length,
                thickValue,
                colorValue.RColor
            );
        }

        EndDrawing();
    }

    public void DrawSplineBasis(
        IEnumerable<Vector2> points,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        DrawSplineBasisSpan(points.AsSpan(), color, thick, camera);
    }

    public void DrawSplineBasisSpan(
        ReadOnlySpan<Vector2> points,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill;
        var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
        if (colorValue == Color.Transparent || points.Length < 4 || thickValue <= 0)
            return;
        BeginDrawing(camera);
        fixed (Vector2* pointsBuffer = points)
        {
            Raylib.DrawSplineBasis(
                (System.Numerics.Vector2*)pointsBuffer,
                points.Length,
                thickValue,
                colorValue.RColor
            );
        }

        EndDrawing();
    }

    public void DrawSplineCatmullRom(
        IEnumerable<Vector2> points,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        DrawSplineCatmullRomSpan(points.AsSpan(), color, thick, camera);
    }

    public void DrawSplineCatmullRomSpan(
        ReadOnlySpan<Vector2> points,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill;
        var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
        if (colorValue == Color.Transparent || points.Length < 4 || thickValue <= 0)
            return;
        BeginDrawing(camera);
        fixed (Vector2* pointsBuffer = points)
        {
            Raylib.DrawSplineCatmullRom(
                (System.Numerics.Vector2*)pointsBuffer,
                points.Length,
                thickValue,
                colorValue.RColor
            );
        }

        EndDrawing();
    }

    public void DrawSplineBezierQuadratic(
        IEnumerable<Vector2> points,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        DrawSplineBezierQuadraticSpan(points.AsSpan(), color, thick, camera);
    }

    public void DrawSplineBezierQuadraticSpan(
        ReadOnlySpan<Vector2> points,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill;
        var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
        if (colorValue == Color.Transparent || points.Length < 3 || thickValue <= 0)
            return;
        BeginDrawing(camera);
        fixed (Vector2* pointsBuffer = points)
        {
            Raylib.DrawSplineBezierQuadratic(
                (System.Numerics.Vector2*)pointsBuffer,
                points.Length,
                thickValue,
                colorValue.RColor
            );
        }

        EndDrawing();
    }

    public void DrawSplineBezierCubic(
        IEnumerable<Vector2> points,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        DrawSplineBezierCubicSpan(points.AsSpan(), color, thick, camera);
    }

    public void DrawSplineBezierCubicSpan(
        ReadOnlySpan<Vector2> points,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill;
        var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
        if (colorValue == Color.Transparent || points.Length < 4 || thickValue <= 0)
            return;
        BeginDrawing(camera);
        fixed (Vector2* pointsBuffer = points)
        {
            Raylib.DrawSplineBezierCubic(
                (System.Numerics.Vector2*)pointsBuffer,
                points.Length,
                thickValue,
                colorValue.RColor
            );
        }

        EndDrawing();
    }

    public void DrawSplineSegmentLinear(
        Vector2 p1,
        Vector2 p2,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill;
        var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
        if (colorValue == Color.Transparent || thickValue <= 0)
            return;
        BeginDrawing(camera);
        Raylib.DrawSplineSegmentLinear(p1, p2, thickValue, colorValue.RColor);
        EndDrawing();
    }

    public void DrawSplineSegmentBasis(
        Vector2 p1,
        Vector2 p2,
        Vector2 p3,
        Vector2 p4,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill;
        var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
        if (colorValue == Color.Transparent || thickValue <= 0)
            return;
        BeginDrawing(camera);
        Raylib.DrawSplineSegmentBasis(p1, p2, p3, p4, thickValue, colorValue.RColor);
        EndDrawing();
    }

    public void DrawSplineSegmentCatmullRom(
        Vector2 p1,
        Vector2 p2,
        Vector2 p3,
        Vector2 p4,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill;
        var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
        if (colorValue == Color.Transparent || thickValue <= 0)
            return;
        BeginDrawing(camera);
        Raylib.DrawSplineSegmentCatmullRom(p1, p2, p3, p4, thickValue, colorValue.RColor);
        EndDrawing();
    }

    public void DrawSplineSegmentCatmullRom(
        Vector2 p1,
        Vector2 p2,
        Vector2 p3,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill;
        var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
        if (colorValue == Color.Transparent || thickValue <= 0)
            return;
        BeginDrawing(camera);
        Raylib.DrawSplineSegmentBezierQuadratic(p1, p2, p3, thickValue, colorValue.RColor);
        EndDrawing();
    }

    public void DrawSplineSegmentBezierCubic(
        Vector2 p1,
        Vector2 p2,
        Vector2 p3,
        Vector2 p4,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill;
        var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
        if (colorValue == Color.Transparent || thickValue <= 0)
            return;
        BeginDrawing(camera);
        Raylib.DrawSplineSegmentBezierCubic(p1, p2, p3, p4, thickValue, colorValue.RColor);
        EndDrawing();
    }

    #endregion

    #region Misc

    public void ClearBackground(Color? color = null)
    {
        var colorValue = color ?? Drawing.DefaultFill;
        if (colorValue == Color.Transparent)
            return;
        BeginDrawing();
        Raylib.ClearBackground(colorValue.RColor);
        EndDrawing();
    }

    public void DrawPixel(float x, float y, Color? color = null)
    {
        DrawPixel(new Vector2(x, y), color);
    }

    public void DrawPixel(Vector2 position, Color? color = null)
    {
        var colorValue = color ?? Drawing.DefaultFill;
        if (colorValue == Color.Transparent)
            return;
        BeginDrawing();
        Raylib.DrawPixelV(position, colorValue.RColor);
        EndDrawing();
    }

    #endregion

    #region Drawing

    public void BeginDrawing(Camera? camera = null)
    {
        if (_drawing)
            throw new InvalidOperationException("Cannot begin drawing while already drawing.");
        _drawing = true;
        var offset = _buffer is null ? Renderer.Offset : 0;
        var scale = _buffer?.Scale ?? Renderer.Scale;
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
                Raylib.BeginScissorMode(
                    (int)(clip.Value.X + PixelOffset).Round(),
                    (int)(clip.Value.Y + PixelOffset).Round(),
                    (int)(clip.Value.Width + PixelOffset).Round(),
                    (int)(clip.Value.Height + PixelOffset).Round()
                );
        }

        var matrix = GetMatrix();
        Rlgl.PushMatrix();
        Rlgl.Translatef(offset.X, offset.Y, 0);
        Rlgl.Scalef(scale.X, scale.Y, 1);
        Rlgl.Translatef(PixelOffset, PixelOffset, 0);
        if (camera is not null)
            matrix *= camera.Matrix;
        Rlgl.MultMatrixf(
            new Matrix4x4(
                matrix.M11,
                matrix.M12,
                0,
                0,
                matrix.M21,
                matrix.M22,
                0,
                0,
                0,
                0,
                1,
                0,
                matrix.M31,
                matrix.M32,
                0,
                1
            )
        );
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
