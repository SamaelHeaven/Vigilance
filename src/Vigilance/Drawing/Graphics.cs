using System.Numerics;
using Raylib_cs;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Math;
using ZLinq;
using Transform = Vigilance.Math.Transform;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.Drawing;

public sealed unsafe class Graphics
{
    private static RenderTexture? _currentBuffer = null;
    private static Box? _currentClip = null;
    private static BlendMode? _currentBlendMode = null;
    private static Shader? _currentShader = null;
    private BlendMode _blendMode = BlendMode.Alpha;
    private Box? _clip = null;
    private bool _culling = Drawing.DefaultCulling;
    private bool _drawing = false;
    private Matrix3x2 _matrix = Matrix3x2.Identity;
    private ValueStack<Matrix3x2> _matrixStack = new();
    private Shader? _shader = null;
    internal RenderTexture? Buffer;

    internal Graphics(RenderTexture? buffer)
    {
        Buffer = buffer;
    }

    #region Bounds

    public Box GetBounds(Camera? camera = null, float offset = 0)
    {
        return GetBounds(GetMatrix(camera), offset);
    }

    public Box GetBounds(in Matrix3x2 matrix, float offset = 0)
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
            (clip?.Size ?? Buffer?.Size ?? Display.Size) + offset * 2
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

    public bool IsBoxInBounds(in Box box, Camera? camera, float offset = 0)
    {
        var matrix = GetMatrix(camera);
        return Collision.CheckPolygonsSpan(box.Transform(matrix), new Quad(GetBounds(matrix, offset)));
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

        return Collision.CheckPolygonsSpan(points, new Quad(GetBounds(matrix, offset)));
    }

    #endregion

    #region Matrix

    public ref Matrix3x2 GetMatrix()
    {
        return ref _matrixStack.Count == 0 ? ref _matrix : ref _matrixStack.Peek();
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

    public void PushMatrix(in Matrix3x2 matrix)
    {
        _matrixStack.Push(matrix);
    }

    public Matrix3x2 PopMatrix()
    {
        return _matrixStack.Count != 0 ? _matrixStack.Pop() : _matrix;
    }

    public void MultiplyMatrix(in Matrix3x2 matrix)
    {
        ref var current = ref GetMatrix();
        current = matrix * current;
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

    public void Rotate(float angle, in Vector2? position = null)
    {
        if (Precision.AreEqual(angle, 0))
            return;
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

    public void Transform(in Transform transform)
    {
        var rotation = transform.Rotation;
        var pivotPoint = transform.PivotPoint;
        var position = transform.Position;
        var scale = transform.Scale.Abs();
        Rotate(rotation, pivotPoint);
        Translate(position);
        Scale(scale);
    }

    public void Pivot(in Transform transform, bool translate)
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

    public Box? SetClip(float x, float y, float width, float height)
    {
        var previous = _clip;
        _clip = new Box(x, y, width, height);
        return previous;
    }

    public Box? SetClip(Vector2 position, Vector2 size)
    {
        var previous = _clip;
        _clip = new Box(position, size);
        return previous;
    }

    public Box? SetClip(in Box? clip)
    {
        var previous = _clip;
        _clip = clip;
        return previous;
    }

    public Box? GetClip()
    {
        return _clip;
    }

    #endregion

    #region BlendMode

    public BlendMode SetBlendMode(BlendMode blendMode)
    {
        var previous = _blendMode;
        _blendMode = blendMode;
        return previous;
    }

    public BlendMode GetBlendMode()
    {
        return _blendMode;
    }

    #endregion

    #region Shader

    public Shader? SetShader(Shader? shader)
    {
        var previous = _shader;
        _shader = shader;
        return previous;
    }

    public Shader? GetShader()
    {
        return _shader;
    }

    #endregion

    #region Culling

    public bool SetCulling(bool culling)
    {
        var previous = _culling;
        _culling = culling;
        return previous;
    }

    public bool Culling()
    {
        return _culling;
    }

    #endregion

    #region Rectangle

    public void FillRectangle(float x, float y, float width, float height, Color? color = null, Camera? camera = null)
    {
        FillRectangle(new Vector2(x, y), new Vector2(width, height), color, camera);
    }

    public void FillRectangle(in Box box, Color? color = null, Camera? camera = null)
    {
        FillRectangle(box.Position, box.Size, color, camera);
    }

    public void FillRectangle(Vector2 position, Vector2 size, Color? color = null, Camera? camera = null)
    {
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
        if (colorValue == Color.Transparent || (_culling && !IsBoxInBounds(position, size, camera)))
            return;
        BeginDrawing(camera);
        Raylib.DrawRectangleRec(new Raylib_cs.Rectangle(position, size), colorValue.RColor);
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
        in Box box,
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
        var topLeftColorValue = topLeftColor ?? Drawing.DefaultFill.Or(Color.White);
        var bottomLeftColorValue = bottomLeftColor ?? Drawing.DefaultFill.Or(Color.White);
        var bottomRightColorValue = bottomRightColor ?? Drawing.DefaultFill.Or(Color.White);
        var topRightColorValue = topRightColor ?? Drawing.DefaultFill.Or(Color.White);
        if (
            (
                topLeftColorValue == Color.Transparent
                && bottomLeftColorValue == Color.Transparent
                && bottomRightColorValue == Color.Transparent
                && topRightColorValue == Color.Transparent
            ) || (_culling && !IsBoxInBounds(position, size, camera))
        )
            return;
        BeginDrawing(camera);
        Raylib.DrawRectangleGradientEx(
            new Raylib_cs.Rectangle(position, size),
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

    public void StrokeRectangle(in Box box, Color? color = null, float? strokeWidth = null, Camera? camera = null)
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
        var colorValue = color ?? Drawing.DefaultStroke.Or(Color.White);
        var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
        if (
            colorValue == Color.Transparent
            || strokeWidthValue <= 0
            || (_culling && !IsBoxInBounds(position, size, camera))
        )
            return;
        BeginDrawing(camera);
        Raylib.DrawRectangleLinesEx(new Raylib_cs.Rectangle(position, size), strokeWidthValue, colorValue.RColor);
        EndDrawing();
    }

    public void FillRoundedRectangle(
        float x,
        float y,
        float width,
        float height,
        Color? color = null,
        float? radius = null,
        Camera? camera = null
    )
    {
        FillRoundedRectangle(new Vector2(x, y), new Vector2(width, height), color, radius, camera);
    }

    public void FillRoundedRectangle(in Box box, Color? color = null, float? radius = null, Camera? camera = null)
    {
        FillRoundedRectangle(box.Position, box.Size, color, radius, camera);
    }

    public void FillRoundedRectangle(
        Vector2 position,
        Vector2 size,
        Color? color = null,
        float? radius = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
        var radiusValue = radius ?? Drawing.DefaultRadius.Or(1f);
        if (colorValue == Color.Transparent || radiusValue <= 0 || (_culling && !IsBoxInBounds(position, size, camera)))
            return;
        BeginDrawing(camera);
        Raylib.DrawRectangleRounded(
            new Raylib_cs.Rectangle(position, size),
            radiusValue == 0 ? 0 : radiusValue / size.X.Abs().Min(size.Y.Abs()),
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
        float? radius = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        StrokeRoundedRectangle(new Vector2(x, y), new Vector2(width, height), color, radius, strokeWidth, camera);
    }

    public void StrokeRoundedRectangle(
        in Box box,
        Color? color = null,
        float? radius = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        StrokeRoundedRectangle(box.Position, box.Size, color, radius, strokeWidth, camera);
    }

    public void StrokeRoundedRectangle(
        Vector2 position,
        Vector2 size,
        Color? color = null,
        float? radius = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultStroke.Or(Color.White);
        var radiusValue = radius ?? Drawing.DefaultRadius.Or(0.1f);
        var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
        if (
            colorValue == Color.Transparent
            || radiusValue <= 0
            || strokeWidthValue <= 0
            || (_culling && !IsBoxInBounds(position, size, camera, strokeWidthValue))
        )
            return;
        BeginDrawing(camera);
        Raylib.DrawRectangleRoundedLinesEx(
            new Raylib_cs.Rectangle(position, size),
            radiusValue == 0 ? 0 : radiusValue / size.X.Abs().Min(size.Y.Abs()),
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

    public void DrawRectangle(in Box box, Rectangle rectangle)
    {
        DrawRectangle(box.Position, box.Size, rectangle);
    }

    public void DrawRectangle(in Transform transform, Rectangle rectangle)
    {
        var camera = rectangle.Camera.Get();
        var fill = rectangle.Fill;
        var stroke = rectangle.Stroke;
        var radius = rectangle.Radius.Abs();
        var position = transform.Position;
        var scale = transform.Scale.Abs();
        var strokeWidth = rectangle.StrokeWidth.Clamp(0, scale.X.Min(scale.Y) * 0.5f);
        var order = rectangle.DrawOrder;
        PushMatrix();
        Pivot(transform, true);
        if (radius > 0)
        {
            position += strokeWidth;
            scale -= strokeWidth * 2;
            if (order == DrawOrder.StrokeThenFill)
            {
                StrokeRoundedRectangle(position, scale, stroke, radius, strokeWidth, camera);
                FillRoundedRectangle(position, scale, fill, radius, camera);
            }
            else
            {
                FillRoundedRectangle(position, scale, fill, radius, camera);
                StrokeRoundedRectangle(position, scale, stroke, radius, strokeWidth, camera);
            }
        }
        else
        {
            if (order == DrawOrder.StrokeThenFill)
            {
                StrokeRectangle(position, scale, stroke, strokeWidth, camera);
                FillRectangle(position + strokeWidth, scale - strokeWidth * 2, fill, camera);
            }
            else
            {
                FillRectangle(position + strokeWidth, scale - strokeWidth * 2, fill, camera);
                StrokeRectangle(position, scale, stroke, strokeWidth, camera);
            }
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

    public void DrawRectangleGradient(in Box box, RectangleGradient rectangle)
    {
        DrawRectangleGradient(box.Position, box.Size, rectangle);
    }

    public void DrawRectangleGradient(in Transform transform, RectangleGradient rectangle)
    {
        var camera = rectangle.Camera.Get();
        var topLeftFill = rectangle.TopLeftFill;
        var bottomLeftFill = rectangle.BottomLeftFill;
        var bottomRightFill = rectangle.BottomRightFill;
        var topRightFill = rectangle.TopRightFill;
        var stroke = rectangle.Stroke;
        var position = transform.Position;
        var scale = transform.Scale.Abs();
        var strokeWidth = rectangle.StrokeWidth.Clamp(0, scale.X.Min(scale.Y) * 0.5f);
        var order = rectangle.DrawOrder;
        PushMatrix();
        Pivot(transform, true);
        if (order == DrawOrder.StrokeThenFill)
        {
            StrokeRectangle(position, scale, stroke, strokeWidth, camera);
            FillRectangleGradient(
                position + strokeWidth,
                scale - strokeWidth * 2,
                topLeftFill,
                bottomLeftFill,
                bottomRightFill,
                topRightFill,
                camera
            );
        }
        else
        {
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
        }

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
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
        if (
            colorValue == Color.Transparent
            || (_culling && !IsBoxInBounds(center - radius, new Vector2(radius * 2), camera))
        )
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
        var innerColorValue = innerColor ?? Drawing.DefaultFill.Or(Color.White);
        var outerColorValue = outerColor ?? Drawing.DefaultFill.Or(Color.White);
        if (
            (innerColorValue == Color.Transparent && outerColorValue == Color.Transparent)
            || (_culling && !IsBoxInBounds(center - radius, new Vector2(radius * 2), camera))
        )
            return;
        BeginDrawing(camera);
        Raylib.DrawCircleGradient(center, radius, innerColorValue.RColor, outerColorValue.RColor);
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
        var colorValue = color ?? Drawing.DefaultStroke.Or(Color.White);
        var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
        if (
            colorValue == Color.Transparent
            || strokeWidthValue <= 0
            || (_culling && !IsBoxInBounds(center - radius, new Vector2(radius * 2), camera))
        )
            return;
        BeginDrawing(camera);
        Raylib.DrawRing(center, radius - strokeWidthValue, radius + 1, 0, 360, 0, colorValue.RColor);
        EndDrawing();
    }

    public void DrawCircle(in Transform transform, Circle circle)
    {
        var camera = circle.Camera.Get();
        var fill = circle.Fill;
        var stroke = circle.Stroke;
        var strokeWidth = circle.StrokeWidth;
        var order = circle.DrawOrder;
        var position = transform.Position;
        var scale = transform.Scale;
        var radius = scale.X.Abs().Min(scale.Y.Abs()) * 0.5f;
        PushMatrix();
        Pivot(transform, false);
        if (order == DrawOrder.StrokeThenFill)
        {
            StrokeCircle(position, radius, stroke, strokeWidth, camera);
            FillCircle(position, radius, fill, camera);
        }
        else
        {
            FillCircle(position, radius, fill, camera);
            StrokeCircle(position, radius, stroke, strokeWidth, camera);
        }

        PopMatrix();
    }

    public void DrawCircleGradient(in Transform transform, CircleGradient circle)
    {
        var camera = circle.Camera.Get();
        var innerFill = circle.InnerFill;
        var outerFill = circle.OuterFill;
        var stroke = circle.Stroke;
        var strokeWidth = circle.StrokeWidth;
        var order = circle.DrawOrder;
        var position = transform.Position;
        var scale = transform.Scale;
        var radius = scale.X.Abs().Min(scale.Y.Abs()) * 0.5f;
        PushMatrix();
        Pivot(transform, false);
        if (order == DrawOrder.StrokeThenFill)
        {
            StrokeCircle(position, radius, stroke, strokeWidth, camera);
            FillCircleGradient(position, radius, innerFill, outerFill, camera);
        }
        else
        {
            FillCircleGradient(position, radius, innerFill, outerFill, camera);
            StrokeCircle(position, radius, stroke, strokeWidth, camera);
        }

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

    public void DrawTriangle(in Transform transform, Triangle triangle)
    {
        var camera = triangle.Camera.Get();
        var position = transform.Position;
        var scale = transform.Scale;
        var fill = triangle.Fill;
        var stroke = triangle.Stroke;
        var strokeWidth = triangle.StrokeWidth;
        var order = triangle.DrawOrder;
        PushMatrix();
        Pivot(transform, false);
        var points = stackalloc Vector2[3];
        var i = 0;
        foreach (var point in triangle.Points)
            points[i++] = point;
        var span = new Span<Vector2>(points, 3);
        Coordinates.Scale(span, scale, position);
        if (order == DrawOrder.StrokeThenFill)
        {
            StrokeCustomPolygonSpan(span, stroke, strokeWidth, camera);
            FillCustomPolygonSpan(span, fill, camera);
        }
        else
        {
            FillCustomPolygonSpan(span, fill, camera);
            StrokeCustomPolygonSpan(span, stroke, strokeWidth, camera);
        }

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
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
        if (
            color == Color.Transparent
            || sides < 3
            || (_culling && !IsBoxInBounds(center - radius, new Vector2(radius * 2), camera))
        )
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
        var colorValue = color ?? Drawing.DefaultStroke.Or(Color.White);
        var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
        if (
            colorValue == Color.Transparent
            || sides < 3
            || strokeWidthValue <= 0
            || (_culling && !IsBoxInBounds(center - radius, new Vector2(radius * 2), camera))
        )
            return;
        BeginDrawing(camera);
        Raylib.DrawPolyLinesEx(center, sides, radius, 0, radius.Min(strokeWidthValue), colorValue.RColor);
        EndDrawing();
    }

    public void DrawRegularPolygon(in Transform transform, RegularPolygon polygon)
    {
        var camera = polygon.Camera.Get();
        var sides = polygon.Sides;
        var fill = polygon.Fill;
        var stroke = polygon.Stroke;
        var strokeWidth = polygon.StrokeWidth;
        var order = polygon.DrawOrder;
        var position = transform.Position;
        var scale = transform.Scale;
        PushMatrix();
        Pivot(transform, false);
        var radius = scale.X.Abs().Min(scale.Y.Abs()) * 0.5f;
        if (order == DrawOrder.StrokeThenFill)
        {
            StrokeRegularPolygon(position, sides, radius, stroke, strokeWidth, camera);
            FillRegularPolygon(position, sides, radius, fill, camera);
        }
        else
        {
            FillRegularPolygon(position, sides, radius, fill, camera);
            StrokeRegularPolygon(position, sides, radius, stroke, strokeWidth, camera);
        }

        PopMatrix();
    }

    public void FillCustomPolygon(IEnumerable<Vector2> points, Color? color = null, Camera? camera = null)
    {
        FillCustomPolygonSpan(points.AsSpan(), color, camera);
    }

    public void FillCustomPolygonSpan(in ReadOnlySpan<Vector2> points, Color? color = null, Camera? camera = null)
    {
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
        if (
            colorValue == Color.Transparent
            || points.Length < 3
            || (_culling && !IsPolygonInBoundsSpan(points, camera))
        )
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
        in ReadOnlySpan<Vector2> points,
        Color? color = null,
        float? strokeWidth = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultStroke.Or(Color.White);
        var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
        if (
            colorValue == Color.Transparent
            || strokeWidthValue <= 0
            || points.Length < 3
            || (_culling && !IsPolygonInBoundsSpan(points, camera, strokeWidthValue * 0.5f))
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

    public void DrawCustomPolygon(in Transform transform, CustomPolygon polygon)
    {
        var camera = polygon.Camera.Get();
        var position = transform.Position;
        var scale = transform.Scale;
        var fill = polygon.Fill;
        var stroke = polygon.Stroke;
        var strokeWidth = polygon.StrokeWidth;
        var order = polygon.DrawOrder;
        PushMatrix();
        Pivot(transform, false);
        PooledArray<Vector2>? pooledArray = null;
        try
        {
            Span<Vector2> span;
            if (polygon.Points.Count > 128)
            {
                pooledArray = polygon.Points.AsValueEnumerable().ToArrayPool();
                span = pooledArray.Value.Span;
            }
            else
            {
                var points = stackalloc Vector2[polygon.Points.Count];
                var i = 0;
                foreach (var point in polygon.Points)
                    points[i++] = point;
                span = new Span<Vector2>(points, polygon.Points.Count);
            }

            Coordinates.Scale(span, scale, position);
            if (order == DrawOrder.StrokeThenFill)
            {
                StrokeCustomPolygonSpan(span, stroke, strokeWidth, camera);
                FillCustomPolygonSpan(span, fill, camera);
            }
            else
            {
                FillCustomPolygonSpan(span, fill, camera);
                StrokeCustomPolygonSpan(span, stroke, strokeWidth, camera);
            }

            PopMatrix();
        }
        finally
        {
            pooledArray?.Dispose();
        }
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
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
        var radius = innerRadius.Max(outerRadius);
        if (
            colorValue == Color.Transparent
            || (_culling && !IsBoxInBounds(center - radius, new Vector2(radius * 2), camera))
        )
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
        var colorValue = color ?? Drawing.DefaultStroke.Or(Color.White);
        var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
        var maxRadius = innerRadius.Max(outerRadius);
        var minRadius = innerRadius.Min(outerRadius);
        if (
            colorValue == Color.Transparent
            || strokeWidthValue <= 0
            || (_culling && !IsBoxInBounds(center - maxRadius, new Vector2(maxRadius * 2), camera, strokeWidthValue))
        )
            return;
        var startDirection = startAngle.Min(endAngle).DegToDirection();
        var endDirection = endAngle.Max(startAngle).DegToDirection();
        var startTangent = new Vector2(-startDirection.Y, startDirection.X);
        var endTangent = new Vector2(-endDirection.Y, endDirection.X);
        var startInner = center + startDirection * (minRadius - strokeWidthValue);
        var endInner = center + endDirection * (minRadius - strokeWidthValue);
        var startOuter = center + startDirection * (maxRadius + strokeWidthValue);
        var endOuter = center + endDirection * (maxRadius + strokeWidthValue);
        var startOffset = startTangent * (strokeWidthValue * 0.5f);
        var endOffset = endTangent * (strokeWidthValue * 0.5f);
        BeginDrawing(camera);
        Raylib.DrawRing(center, maxRadius, maxRadius + strokeWidthValue, startAngle, endAngle, 0, colorValue.RColor);
        Raylib.DrawRing(center, minRadius - strokeWidthValue, minRadius, startAngle, endAngle, 0, colorValue.RColor);
        Raylib.DrawLineEx(startInner - startOffset, startOuter - startOffset, strokeWidthValue, colorValue.RColor);
        Raylib.DrawLineEx(endInner + endOffset, endOuter + endOffset, strokeWidthValue, colorValue.RColor);
        EndDrawing();
    }

    public void DrawRing(in Transform transform, Ring ring)
    {
        var camera = ring.Camera.Get();
        var startAngle = ring.StartAngle;
        var endAngle = ring.EndAngle;
        var fill = ring.Fill;
        var stroke = ring.Stroke;
        var strokeWidth = ring.StrokeWidth;
        var order = ring.DrawOrder;
        var position = transform.Position;
        var scale = transform.Scale.X.Abs().Min(transform.Scale.Y.Abs());
        var innerRadius = ring.InnerRadius * scale;
        var outerRadius = ring.OuterRadius * scale;
        PushMatrix();
        Pivot(transform, false);
        if (order == DrawOrder.StrokeThenFill)
        {
            StrokeRing(position, innerRadius, outerRadius, startAngle, endAngle, stroke, strokeWidth, camera);
            FillRing(position, innerRadius, outerRadius, startAngle, endAngle, fill, camera);
        }
        else
        {
            FillRing(position, innerRadius, outerRadius, startAngle, endAngle, fill, camera);
            StrokeRing(position, innerRadius, outerRadius, startAngle, endAngle, stroke, strokeWidth, camera);
        }

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
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
        var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
        if (
            colorValue == Color.Transparent
            || thickValue <= 0
            || (_culling && !IsPolygonInBoundsSpan(new Quad(start, start, end, end), camera, thickValue * 0.5f))
        )
            return;
        BeginDrawing(camera);
        Raylib.DrawLineEx(start, end, thickValue, colorValue.RColor);
        EndDrawing();
    }

    public void DrawLine(in Transform transform, Line line)
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
        in Vector2? spacing = null,
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
        in Vector2? spacing = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
        if (text.IsEmpty || colorValue == Color.Transparent)
            return;
        font ??= Font.Default;
        Raylib.SetTextureFilter(font.Atlas.Texture2D, (TextureFilter)(interpolation ?? Drawing.DefaultInterpolation));
        BeginDrawing(camera);
        foreach (var (source, dest) in font.GetTextBounds(text, fontSize, spacing))
        {
            var finalDest = new Box(
                dest.Position.X + position.X,
                dest.Position.Y + position.Y,
                dest.Size.X,
                dest.Size.Y
            );
            if (_culling && !IsBoxInBounds(finalDest, camera))
                continue;
            Raylib.DrawTexturePro(
                font.Atlas.Texture2D,
                new Raylib_cs.Rectangle(source.X, source.Y, source.Width, source.Height),
                new Raylib_cs.Rectangle(finalDest.Position.X, finalDest.Position.Y, finalDest.Size.X, finalDest.Size.Y),
                Vector2.Zero,
                0,
                colorValue.RColor
            );
        }

        EndDrawing();
    }

    public void StrokeText(
        string text,
        float x,
        float y,
        Color? color = null,
        Font? font = null,
        float? fontSize = null,
        float? strokeWidth = null,
        in Vector2? spacing = null,
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
        in Vector2? spacing = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultStroke.Or(Color.White);
        var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
        if (text.IsEmpty || colorValue == Color.Transparent || strokeWidthValue <= 0)
            return;
        font ??= Font.Default;
        var (atlas, glyphInfos) = font.GetStroke((int)strokeWidthValue.Ceil());
        Raylib.SetTextureFilter(atlas.Texture2D, (TextureFilter)(interpolation ?? Drawing.DefaultInterpolation));
        BeginDrawing(camera);
        foreach (var (source, dest) in font.GetTextBounds(text, fontSize, spacing, glyphInfos))
        {
            var finalDest = new Box(
                dest.Position.X + position.X,
                dest.Position.Y + position.Y,
                dest.Size.X,
                dest.Size.Y
            );
            if (_culling && !IsBoxInBounds(finalDest, camera))
                continue;
            Raylib.DrawTexturePro(
                atlas.Texture2D,
                new Raylib_cs.Rectangle(source.X, source.Y, source.Width, source.Height),
                new Raylib_cs.Rectangle(finalDest.Position.X, finalDest.Position.Y, finalDest.Size.X, finalDest.Size.Y),
                Vector2.Zero,
                0,
                colorValue.RColor
            );
        }

        EndDrawing();
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
        var order = text.DrawOrder;
        var position = transform.Position;
        var scale = (transform.Scale.X.Abs() + transform.Scale.Y.Abs()) * 0.5f;
        var size = text.Size;
        fontSize *= scale;
        transform.Scale = size;
        PushMatrix();
        Pivot(transform, true);
        if (!_culling || IsBoxInBounds(position, size * scale, camera, strokeWidth * 0.5f))
        {
            if (order == DrawOrder.StrokeThenFill)
            {
                StrokeText(value, position, stroke, font, fontSize, strokeWidth, spacing, interpolation, camera);
                FillText(value, position, fill, font, fontSize, spacing, interpolation, camera);
            }
            else
            {
                FillText(value, position, fill, font, fontSize, spacing, interpolation, camera);
                StrokeText(value, position, stroke, font, fontSize, strokeWidth, spacing, interpolation, camera);
            }
        }

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
        in Box box,
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
        in Vector2? size = null,
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
        in Box source,
        in Box dest,
        Color? tint = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        var tintValue = tint ?? Color.White;
        if (tintValue == Color.Transparent || texture == Texture.Empty || (_culling && !IsBoxInBounds(dest, camera)))
            return;
        var rSource = new Raylib_cs.Rectangle(
            source.X,
            source.Y,
            source.Width,
            texture.RenderTexture is null ? source.Height : -source.Height
        );
        var rDest = new Raylib_cs.Rectangle(dest.Position, dest.Size);
        Raylib.SetTextureFilter(texture.Texture2D, (TextureFilter)(interpolation ?? Drawing.DefaultInterpolation));
        BeginDrawing(camera);
        Raylib.DrawTexturePro(texture.Texture2D, rSource, rDest, Vector2.Zero, 0, tintValue.RColor);
        EndDrawing();
    }

    public void DrawTextureNPatch(
        Texture texture,
        in NPatchInfo nPatchInfo,
        float x,
        float y,
        Color? tint = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        DrawTextureNPatch(texture, nPatchInfo, new Vector2(x, y), null, tint, interpolation, camera);
    }

    public void DrawTextureNPatch(
        Texture texture,
        in NPatchInfo nPatchInfo,
        float x,
        float y,
        float width,
        float height,
        Color? tint = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        DrawTextureNPatch(
            texture,
            nPatchInfo,
            new Vector2(x, y),
            new Vector2(width, height),
            tint,
            interpolation,
            camera
        );
    }

    public void DrawTextureNPatch(
        Texture texture,
        in NPatchInfo nPatchInfo,
        in Box box,
        Color? tint = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        DrawTextureNPatch(texture, nPatchInfo, box.Position, box.Size, tint, interpolation, camera);
    }

    public void DrawTextureNPatch(
        Texture texture,
        in NPatchInfo nPatchInfo,
        Vector2 position,
        in Vector2? size = null,
        Color? tint = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        DrawTextureNPatch(
            texture,
            nPatchInfo,
            new Box(Vector2.Zero, texture.Size),
            new Box(position, size ?? texture.Size),
            tint,
            interpolation,
            camera
        );
    }

    public void DrawTextureNPatch(
        Texture texture,
        in NPatchInfo nPatchInfo,
        in Box source,
        in Box dest,
        Color? tint = null,
        Interpolation? interpolation = null,
        Camera? camera = null
    )
    {
        var tintValue = tint ?? Color.White;
        if (tintValue == Color.Transparent || texture == Texture.Empty || (_culling && !IsBoxInBounds(dest, camera)))
            return;
        var rSource = new Raylib_cs.Rectangle(
            source.X,
            source.Y,
            source.Width,
            texture.RenderTexture is null ? source.Height : -source.Height
        );
        var rDest = new Raylib_cs.Rectangle(dest.Position, dest.Size);
        var rNPatchInfo = new Raylib_cs.NPatchInfo
        {
            Source = rSource,
            Left = nPatchInfo.Left,
            Top = nPatchInfo.Top,
            Right = nPatchInfo.Right,
            Bottom = nPatchInfo.Bottom,
            Layout = (Raylib_cs.NPatchLayout)nPatchInfo.Layout,
        };
        Raylib.SetTextureFilter(texture.Texture2D, (TextureFilter)(interpolation ?? Drawing.DefaultInterpolation));
        BeginDrawing(camera);
        Raylib.DrawTextureNPatch(texture.Texture2D, rNPatchInfo, rDest, Vector2.Zero, 0, tintValue.RColor);
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

    public void DrawSprite(in Box box, Sprite sprite)
    {
        DrawSprite(box.Position, box.Size, sprite);
    }

    public void DrawSprite(in Transform transform, Sprite sprite)
    {
        var camera = sprite.Camera.Get();
        var texture = sprite.Texture;
        var interpolation = sprite.Interpolation;
        var tint = sprite.Tint;
        var nPatchInfo = sprite.NPatchInfo;
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
        if (nPatchInfo.HasValue)
            DrawTextureNPatch(texture, nPatchInfo.Value, source, new Box(position, scale), tint, interpolation, camera);
        else
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

    public void DrawGrid(in Box box, float cellSize, Color? color = null, float? thick = null, Camera? camera = null)
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
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
        var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
        if (
            colorValue == Color.Transparent
            || thickValue <= 0
            || (_culling && !IsBoxInBounds(position, size, camera, thickValue * 0.5f))
        )
            return;
        cellSize = cellSize.Max(1);
        BeginDrawing(camera);
        for (var x = position.X; x <= position.X + size.X; x += cellSize)
            Raylib.DrawLineEx(
                new Vector2(x, position.Y),
                new Vector2(x, position.Y + size.Y),
                thickValue,
                colorValue.RColor
            );
        for (var y = position.Y; y <= position.Y + size.Y; y += cellSize)
            Raylib.DrawLineEx(
                new Vector2(position.X, y),
                new Vector2(position.X + size.X, y),
                thickValue,
                colorValue.RColor
            );
        EndDrawing();
    }

    public void DrawGrid(float x, float y, float width, float height, Grid grid)
    {
        DrawGrid(new Vector2(x, y), new Vector2(width, height), grid);
    }

    public void DrawGrid(Vector2 position, Vector2 size, Grid grid)
    {
        DrawGrid(new Transform(position + size * 0.5f, size), grid);
    }

    public void DrawGrid(in Box box, Grid grid)
    {
        DrawGrid(box.Position, box.Size, grid);
    }

    public void DrawGrid(in Transform transform, Grid grid)
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
        in ReadOnlySpan<Vector2> points,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
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
        in ReadOnlySpan<Vector2> points,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
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
        in ReadOnlySpan<Vector2> points,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
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
        in ReadOnlySpan<Vector2> points,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
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
        in ReadOnlySpan<Vector2> points,
        Color? color = null,
        float? thick = null,
        Camera? camera = null
    )
    {
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
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
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
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
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
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
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
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
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
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
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
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
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
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
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
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
        var offset = Buffer is null ? Renderer.Offset : 0;
        var scale = Buffer?.Scale ?? Renderer.Scale;
        if (_currentBuffer != Buffer)
        {
            if (_currentBuffer is null)
                DrawCurrentBuffer();
            else
                Raylib.EndTextureMode();
            _currentBuffer = Buffer;
            if (Buffer is not null)
                Raylib.BeginTextureMode(Buffer.RenderTexture2D);
        }

        var clip = _clip;
        if (clip.HasValue)
            clip = new Box(clip.Value.Position * scale + offset, clip.Value.Size * scale);
        if (!Precision.AreEqual(_currentClip, clip))
        {
            if (_currentClip.HasValue)
                Raylib.EndScissorMode();
            _currentClip = clip;
            if (clip.HasValue)
                Raylib.BeginScissorMode(
                    (int)clip.Value.X.Round(),
                    (int)clip.Value.Y.Round(),
                    (int)clip.Value.Width.Round(),
                    (int)clip.Value.Height.Round()
                );
        }

        if (_currentBlendMode != _blendMode)
        {
            DrawCurrentBuffer();
            Rlgl.SetBlendFactorsSeparate(
                (int)_blendMode.SrcRgb,
                (int)_blendMode.DstRgb,
                (int)_blendMode.SrcAlpha,
                (int)_blendMode.DstAlpha,
                (int)_blendMode.EqRgb,
                (int)_blendMode.EqAlpha
            );
            _currentBlendMode = _blendMode;
            Raylib.BeginBlendMode(Raylib_cs.BlendMode.CustomSeparate);
        }

        if (_currentShader != _shader)
        {
            if (_currentShader is not null)
                Raylib.EndShaderMode();
            _currentShader = _shader;
            if (_shader is not null)
                Raylib.BeginShaderMode(_shader.RShader);
        }

        var matrix = GetMatrix(camera);
        matrix *= Matrix3x2.CreateScale(scale) * Matrix3x2.CreateTranslation(offset);
        Rlgl.PushMatrix();
        var float16 =
            stackalloc float[16] {
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
                1,
            };
        Rlgl.MultMatrixf(float16);
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

    internal static bool IsBufferCurrent(RenderTexture? buffer)
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

        if (_currentClip.HasValue)
        {
            Raylib.EndScissorMode();
            _currentClip = null;
        }

        if (_currentBlendMode.HasValue)
        {
            Raylib.EndBlendMode();
            _currentBlendMode = null;
        }

        if (_currentShader is not null)
        {
            Raylib.EndShaderMode();
            _currentShader = null;
        }

        Rlgl.LoadIdentity();
    }

    internal static void DrawCurrentBuffer()
    {
        Rlgl.DrawRenderBatchActive();
    }

    #endregion
}
