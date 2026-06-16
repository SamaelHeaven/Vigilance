using System.Buffers;
using System.Numerics;
using Raylib_cs;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Math;
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
    private ValueStack<Matrix3x2> _matrices = new();
    private Matrix3x2 _matrix = Matrix3x2.Identity;
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
            var scale = matrix.GetScale();
            offset *= scale.Max();
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
        Vector2[]? pooledPoints = null;
        try
        {
            if (points.Length > 128)
            {
                pooledPoints = ArrayPool<Vector2>.Shared.Rent(points.Length);
                for (var i = 0; i < points.Length; i++)
                    pooledPoints[i] = points[i].Transform(matrix);
                points = pooledPoints.AsSpan(0, points.Length);
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
        finally
        {
            if (pooledPoints is not null)
                ArrayPool<Vector2>.Shared.Return(pooledPoints);
        }
    }

    #endregion

    #region Matrix

    public ref Matrix3x2 GetMatrix()
    {
        return ref _matrices.Count == 0 ? ref _matrix : ref _matrices.Peek();
    }

    public Matrix3x2 GetMatrix(Camera? camera)
    {
        return camera is not null ? GetMatrix() * camera.Matrix : GetMatrix();
    }

    public void LoadIdentity()
    {
        _matrices.Clear();
        _matrix = Matrix3x2.Identity;
    }

    public void PushMatrix()
    {
        _matrices.Push(GetMatrix());
    }

    public void PushMatrix(in Matrix3x2 matrix)
    {
        _matrices.Push(matrix);
    }

    public Matrix3x2 PopMatrix()
    {
        return _matrices.Count != 0 ? _matrices.Pop() : _matrix;
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
        texture.Interpolation = interpolation ?? Drawing.DefaultInterpolation;
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
        texture.Interpolation = interpolation ?? Drawing.DefaultInterpolation;
        BeginDrawing(camera);
        Raylib.DrawTextureNPatch(texture.Texture2D, rNPatchInfo, rDest, Vector2.Zero, 0, tintValue.RColor);
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
                    (int)clip.Value.X.Floor(),
                    (int)clip.Value.Y.Floor(),
                    (int)clip.Value.Width.Ceil(),
                    (int)clip.Value.Height.Ceil()
                );
        }

        if (_currentBlendMode != _blendMode)
        {
            DrawCurrentBuffer();
            Rlgl.SetBlendFactorsSeparate(
                _blendMode.SrcRgb.ToGL(),
                _blendMode.DstRgb.ToGL(),
                _blendMode.SrcAlpha.ToGL(),
                _blendMode.DstAlpha.ToGL(),
                _blendMode.EqRgb.ToGL(),
                _blendMode.EqAlpha.ToGL()
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
