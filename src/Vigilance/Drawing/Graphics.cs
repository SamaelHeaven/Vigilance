using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

public readonly struct Graphics
{
    internal static WritableTexture? CurrentBuffer;
    public const float DefaultRectangleRoundness = 0.1f;
    public const float DefaultStrokeWidth = 1;
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

    public static void Translate(Vector2 translation)
    {
        Game.EnsureRunning();
        Rlgl.Translatef(translation.X, translation.Y, 0);
    }

    public static void Rotate(float angle)
    {
        Game.EnsureRunning();
        Rlgl.Rotatef(angle, 0, 0, 1);
    }

    public static void Scale(Vector2 scale)
    {
        Game.EnsureRunning();
        Rlgl.Scalef(scale.X, scale.Y, 1);
    }

    public static void Transform(Transform transform)
    {
        Game.EnsureRunning();
        Transform(transform, out _, out var scale);
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
        float strokeWidth = DefaultStrokeWidth,
        Camera? camera = null
    )
    {
        StrokeRectangle(new Vector2(x, y), new Vector2(width, height), color, strokeWidth, camera);
    }

    public void StrokeRectangle(
        Vector2 position,
        Vector2 size,
        Color color,
        float strokeWidth = DefaultStrokeWidth,
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
        float roundness = DefaultRectangleRoundness,
        Camera? camera = null
    )
    {
        FillRoundedRectangle(new Vector2(x, y), new Vector2(width, height), color, roundness, camera);
    }

    public void FillRoundedRectangle(
        Vector2 position,
        Vector2 size,
        Color color,
        float roundness = DefaultRectangleRoundness,
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
        float roundness = DefaultRectangleRoundness,
        float strokeWidth = DefaultStrokeWidth,
        Camera? camera = null
    )
    {
        StrokeRoundedRectangle(new Vector2(x, y), new Vector2(width, height), color, roundness, strokeWidth, camera);
    }

    public void StrokeRoundedRectangle(
        Vector2 position,
        Vector2 size,
        Color color,
        float roundness = DefaultRectangleRoundness,
        float strokeWidth = DefaultStrokeWidth,
        Camera? camera = null
    )
    {
        if (color == Color.Transparent)
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
        var camera = rectangle.Camera();
        var fill = rectangle.Fill;
        var stroke = rectangle.Stroke;
        var roundness = rectangle.Roundness;
        var strokeWidth = rectangle.StrokeWidth;
        PushState();
        Transform(transform, out var position, out var scale);
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

    private static void Transform(Transform transform, out Vector2 position, out Vector2 scale)
    {
        position = transform.Position;
        scale = transform.Scale.Abs();
        var pivotPoint = transform.PivotPoint;
        var rotation = transform.Rotation;
        var positionOffset = -(scale * 0.5f);
        var rotationOffset = position + pivotPoint;
        Translate(rotationOffset);
        Rotate(rotation);
        Translate(-rotationOffset + positionOffset);
    }
}
