using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
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
    private static ShapeTexture? _currentShapesTexture = null;
    private static Texture2D? _defaultShapesTexture = null;
    private static Raylib_cs.Rectangle _defaultShapesTextureSource;
    private readonly bool _primary;
    private BlendMode _blendMode = Drawing.DefaultBlendMode;
    private Box? _clip = null;
    private bool _culling = Drawing.DefaultCulling;
    private bool _drawing = false;
    private ValueStack<Matrix3x2> _matrices = [];
    private Matrix3x2 _matrix = Matrix3x2.Identity;
    private Shader _shader = Drawing.DefaultShader;
    private ShapeTexture? _shapesTexture = null;
    internal RenderTexture? Buffer;

    internal Graphics(RenderTexture? buffer, bool primary = false)
    {
        Buffer = buffer;
        _primary = primary;
    }

    private static Shader CurrentShader
    {
        get => field ??= Shader.Default;
        set;
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
            (clip?.Size ?? (_primary ? Display.Size : Buffer?.Size) ?? Display.Size) + offset * 2
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
        return Collision.CheckPolygons(box.Transform(matrix), new Quad(GetBounds(matrix, offset)));
    }

    public bool IsPolygonInBounds(IEnumerable<Vector2> points, Camera? camera, float offset = 0)
    {
        return IsPolygonInBounds(points.AsSpan(), camera, offset);
    }

    [OverloadResolutionPriority(1)]
    public bool IsPolygonInBounds(ReadOnlySpan<Vector2> points, Camera? camera, float offset = 0)
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

            return Collision.CheckPolygons(points, new Quad(GetBounds(matrix, offset)));
        }
        finally
        {
            if (pooledPoints is not null)
                ArrayPool<Vector2>.Shared.Return(pooledPoints);
        }
    }

    #endregion

    #region Matrix

    public static Matrix4x4 GetMatrixModelView()
    {
        return Matrix4x4.Transpose(Rlgl.GetMatrixModelview());
    }

    public static Matrix4x4 GetMatrixTransform()
    {
        return Matrix4x4.Transpose(Rlgl.GetMatrixTransform());
    }

    public static Matrix4x4 GetMatrixProjection()
    {
        return Matrix4x4.Transpose(Rlgl.GetMatrixProjection());
    }

    public static Matrix4x4 GetCurrentMatrix()
    {
        return GetMatrixModelView() * GetMatrixTransform() * GetMatrixProjection();
    }

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
        Translate(new Vector2(v1, v2 ?? v1));
    }

    public void Translate(Vector2 translation)
    {
        if (Precision.AreEqual(translation, Vector2.Zero))
            return;
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
        if (Precision.AreEqual(scale, Vector2.One))
            return;
        MultiplyMatrix(Matrix3x2.CreateScale(scale.X, scale.Y));
    }

    public void Skew(float v1, float? v2 = null)
    {
        Skew(new Vector2(v1, v2 ?? v1));
    }

    public void Skew(Vector2 skew)
    {
        if (Precision.AreEqual(skew, Vector2.Zero))
            return;
        MultiplyMatrix(Matrix3x2.CreateSkew(skew.X.DegToRad(), skew.Y.DegToRad()));
    }

    public void Transform(in Transform transform)
    {
        var rotation = transform.Rotation;
        var pivotPoint = transform.PivotPoint;
        var position = transform.Position;
        var scale = transform.Scale.Abs();
        Translate(position);
        Rotate(rotation, pivotPoint);
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

    public Shader SetShader(Shader shader)
    {
        var previous = _shader;
        _shader = shader;
        return previous;
    }

    public Shader GetShader()
    {
        return _shader;
    }

    #endregion

    #region ShapesTexture

    public ShapeTexture? SetShapesTexture(ShapeTexture? shapesTexture)
    {
        var previous = _shapesTexture;
        _shapesTexture = shapesTexture;
        return previous;
    }

    public ShapeTexture? GetShapesTexture()
    {
        return _shapesTexture;
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
        TextureFilter? textureFilter = null,
        TextureWrap? textureWrap = null,
        Camera? camera = null
    )
    {
        DrawTexture(texture, new Vector2(x, y), null, tint, textureFilter, textureWrap, camera);
    }

    public void DrawTexture(
        Texture texture,
        float x,
        float y,
        float width,
        float height,
        Color? tint = null,
        TextureFilter? textureFilter = null,
        TextureWrap? textureWrap = null,
        Camera? camera = null
    )
    {
        DrawTexture(texture, new Vector2(x, y), new Vector2(width, height), tint, textureFilter, textureWrap, camera);
    }

    public void DrawTexture(
        Texture texture,
        in Box box,
        Color? tint = null,
        TextureFilter? textureFilter = null,
        TextureWrap? textureWrap = null,
        Camera? camera = null
    )
    {
        DrawTexture(texture, box.Position, box.Size, tint, textureFilter, textureWrap, camera);
    }

    public void DrawTexture(
        Texture texture,
        Vector2 position,
        in Vector2? size = null,
        Color? tint = null,
        TextureFilter? textureFilter = null,
        TextureWrap? textureWrap = null,
        Camera? camera = null
    )
    {
        DrawTexture(
            texture,
            new Box(Vector2.Zero, texture.Size),
            new Box(position, size ?? texture.Size),
            tint,
            textureFilter,
            textureWrap,
            camera
        );
    }

    public void DrawTexture(
        Texture texture,
        in Box source,
        in Box dest,
        Color? tint = null,
        TextureFilter? textureFilter = null,
        TextureWrap? textureWrap = null,
        Camera? camera = null
    )
    {
        var tintValue = tint ?? Color.White;
        if (tintValue == Color.Transparent || texture == Texture.Empty || (_culling && !IsBoxInBounds(dest, camera)))
            return;
        var rSource = GetTextureSource(texture, source);
        var rDest = new Raylib_cs.Rectangle(dest.Position, dest.Size);
        texture.TextureFilter = textureFilter ?? Drawing.DefaultTextureFilter;
        texture.TextureWrap = textureWrap ?? Drawing.DefaultTextureWrap;
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
        TextureFilter? textureFilter = null,
        TextureWrap? textureWrap = null,
        Camera? camera = null
    )
    {
        DrawTextureNPatch(texture, nPatchInfo, new Vector2(x, y), null, tint, textureFilter, textureWrap, camera);
    }

    public void DrawTextureNPatch(
        Texture texture,
        in NPatchInfo nPatchInfo,
        float x,
        float y,
        float width,
        float height,
        Color? tint = null,
        TextureFilter? textureFilter = null,
        TextureWrap? textureWrap = null,
        Camera? camera = null
    )
    {
        DrawTextureNPatch(
            texture,
            nPatchInfo,
            new Vector2(x, y),
            new Vector2(width, height),
            tint,
            textureFilter,
            textureWrap,
            camera
        );
    }

    public void DrawTextureNPatch(
        Texture texture,
        in NPatchInfo nPatchInfo,
        in Box box,
        Color? tint = null,
        TextureFilter? textureFilter = null,
        TextureWrap? textureWrap = null,
        Camera? camera = null
    )
    {
        DrawTextureNPatch(texture, nPatchInfo, box.Position, box.Size, tint, textureFilter, textureWrap, camera);
    }

    public void DrawTextureNPatch(
        Texture texture,
        in NPatchInfo nPatchInfo,
        Vector2 position,
        in Vector2? size = null,
        Color? tint = null,
        TextureFilter? textureFilter = null,
        TextureWrap? textureWrap = null,
        Camera? camera = null
    )
    {
        DrawTextureNPatch(
            texture,
            nPatchInfo,
            new Box(Vector2.Zero, texture.Size),
            new Box(position, size ?? texture.Size),
            tint,
            textureFilter,
            textureWrap,
            camera
        );
    }

    public void DrawTextureNPatch(
        Texture texture,
        in NPatchInfo nPatchInfo,
        in Box source,
        in Box dest,
        Color? tint = null,
        TextureFilter? textureFilter = null,
        TextureWrap? textureWrap = null,
        Camera? camera = null
    )
    {
        var tintValue = tint ?? Color.White;
        if (tintValue == Color.Transparent || texture == Texture.Empty || (_culling && !IsBoxInBounds(dest, camera)))
            return;
        var rSource = GetTextureSource(texture, source);
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
        texture.TextureFilter = textureFilter ?? Drawing.DefaultTextureFilter;
        texture.TextureWrap = textureWrap ?? Drawing.DefaultTextureWrap;
        BeginDrawing(camera);
        Raylib.DrawTextureNPatch(texture.Texture2D, rNPatchInfo, rDest, Vector2.Zero, 0, tintValue.RColor);
        EndDrawing();
    }

    #endregion

    #region Misc

    public void Reset()
    {
        LoadIdentity();
        _clip = null;
        _blendMode = Drawing.DefaultBlendMode;
        _culling = Drawing.DefaultCulling;
        _shader = Drawing.DefaultShader;
    }

    public void ClearBackground(Color? color = null)
    {
        var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
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

    public static bool IsBufferCurrent(RenderTexture? buffer)
    {
        return _currentBuffer == buffer;
    }

    public static void ResetCurrentBuffer()
    {
        if (_currentClip.HasValue)
        {
            EndClip();
            _currentClip = null;
        }

        if (_currentBuffer is not null)
        {
            Raylib.EndTextureMode();
            _currentBuffer = null;
        }

        if (_currentBlendMode.HasValue)
        {
            Raylib.EndBlendMode();
            _currentBlendMode = null;
        }

        if (!CurrentShader.IsDefault)
        {
            Raylib.EndShaderMode();
            CurrentShader = Shader.Default;
        }

        if (_currentShapesTexture is not null)
        {
            if (_defaultShapesTexture.HasValue)
                Raylib.SetShapesTexture(_defaultShapesTexture.Value, _defaultShapesTextureSource);
            _currentShapesTexture = null;
        }

        Rlgl.LoadIdentity();
    }

    private static void EnsureDefaultShapesTexture()
    {
        if (_defaultShapesTexture.HasValue)
            return;
        _defaultShapesTexture = Raylib.GetShapesTexture();
        _defaultShapesTextureSource = Raylib.GetShapesTextureRectangle();
    }

    private static bool ShapesTextureEquals(ShapeTexture? a, ShapeTexture? b)
    {
        if (a is not { } av)
            return b is null;
        return b is { } bv && ReferenceEquals(av.Texture, bv.Texture) && Precision.AreEqual(av.Source, bv.Source);
    }

    public static void DrawCurrentBuffer()
    {
        Rlgl.DrawRenderBatchActive();
    }

    #endregion

    #region Drawing

    public void BeginDrawing(Camera? camera = null)
    {
        if (_drawing)
            throw new InvalidOperationException("Cannot begin drawing while already drawing.");
        _drawing = true;
        Vector2 offset;
        Vector2 scale;
        if (_primary)
        {
            var bufferScale = Buffer?.Scale ?? 1f;
            offset = Renderer.Offset * bufferScale;
            scale = Renderer.Scale * bufferScale;
        }
        else
        {
            offset = Vector2.Zero;
            scale = Buffer!.Scale;
        }

        if (_currentBuffer != Buffer)
        {
            if (_currentBuffer is null)
                DrawCurrentBuffer();
            else
                Raylib.EndTextureMode();
            _currentBuffer = Buffer;
            if (Buffer is not null)
            {
                Raylib.BeginTextureMode(Buffer.RenderTexture2D);
                ApplyLogicalRenderTarget(Buffer);
            }
        }

        var clip = _clip;
        if (clip.HasValue)
            clip = new Box(clip.Value.Position * scale + offset, clip.Value.Size * scale);
        if (!Precision.AreEqual(_currentClip, clip))
        {
            if (_currentClip.HasValue)
                EndClip();
            _currentClip = clip;
            if (clip.HasValue)
                BeginClip(clip.Value);
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

        var currentShader = CurrentShader;
        if (currentShader != _shader)
        {
            if (!currentShader.IsDefault)
                Raylib.EndShaderMode();
            CurrentShader = _shader;
            if (!_shader.IsDefault)
                Raylib.BeginShaderMode(_shader.RShader);
        }

        EnsureDefaultShapesTexture();
        if (!ShapesTextureEquals(_currentShapesTexture, _shapesTexture))
        {
            _currentShapesTexture = _shapesTexture;
            if (_shapesTexture is { } shapesTexture)
            {
                var source = shapesTexture.Source;
                Raylib.SetShapesTexture(
                    shapesTexture.Texture.Texture2D,
                    new Raylib_cs.Rectangle(source.X, source.Y, source.Width, source.Height)
                );
            }
            else
            {
                Raylib.SetShapesTexture(_defaultShapesTexture!.Value, _defaultShapesTextureSource);
            }
        }

        var matrix = GetMatrix(camera);
        matrix *= Matrix3x2.CreateScale(scale) * Matrix3x2.CreateTranslation(offset);
        Rlgl.PushMatrix();
        Rlgl.MultMatrixf(Matrix4x4.Transpose(matrix.ToMatrix4x4()));
    }

    public void EndDrawing()
    {
        if (!_drawing)
            throw new InvalidOperationException($"{nameof(BeginDrawing)} must be called before {nameof(EndDrawing)}.");
        _drawing = false;
        Rlgl.PopMatrix();
    }

    private static void ApplyLogicalRenderTarget(RenderTexture buffer)
    {
        var width = buffer.ScaledWidth;
        var height = buffer.ScaledHeight;
        if (width == buffer.PhysicalWidth && height == buffer.PhysicalHeight)
            return;
        Rlgl.Viewport(0, 0, width, height);
        Rlgl.SetMatrixProjection(
            Matrix4x4.Transpose(Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, 0.0f, 1.0f))
        );
    }

    private static Raylib_cs.Rectangle GetTextureSource(Texture texture, in Box source)
    {
        return !texture.IsRenderTexture
            ? new Raylib_cs.Rectangle(source.X, source.Y, source.Width, source.Height)
            : new Raylib_cs.Rectangle(source.X, source.Y, source.Width, -source.Height);
    }

    private void BeginClip(in Box clip)
    {
        var x = (int)clip.X.Floor();
        var y = (int)clip.Y.Floor();
        var width = (int)clip.Width.Ceil();
        var height = (int)clip.Height.Ceil();
        if (Buffer is null)
        {
            Raylib.BeginScissorMode(x, y, width, height);
            return;
        }

        Rlgl.DrawRenderBatchActive();
        Rlgl.EnableScissorTest();
        Rlgl.Scissor(x, Buffer.ScaledHeight - (y + height), width, height);
    }

    private static void EndClip()
    {
        if (_currentBuffer is null)
        {
            Raylib.EndScissorMode();
        }
        else
        {
            Rlgl.DrawRenderBatchActive();
            Rlgl.DisableScissorTest();
        }
    }

    #endregion
}
