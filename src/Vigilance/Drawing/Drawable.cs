using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public abstract class Drawable : IDrawable, IFullCloneable
{
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; } = 0;
    public Vector2 PivotPoint { get; set; } = Vector2.Zero;
    public BlendMode? BlendMode { get; set; } = null;
    public Shader? Shader { get; set; } = null;
    public bool? Culling { get; set; } = null;

    public Transform Transform
    {
        get => new(Position, Scale, Rotation, PivotPoint);
        set
        {
            Position = value.Position;
            Scale = value.Scale;
            Rotation = value.Rotation;
            PivotPoint = value.PivotPoint;
        }
    }

    public abstract void Draw(Transform transform, Graphics graphics);

    public static DrawScope<T> EnterDrawing<T>(ref Transform transform, T drawable, Graphics graphics)
        where T : Drawable<T>
    {
        var originalTransform = transform;
        drawable.OnBeginDrawing?.Invoke(originalTransform, drawable, graphics);
        BlendMode? previousBlendMode = null;
        Shader? previousShader = null;
        bool? previousCulling = null;
        if (drawable.BlendMode.HasValue)
            previousBlendMode = graphics.SetBlendMode(drawable.BlendMode.Value);
        if (drawable.Shader is not null)
            previousShader = graphics.SetShader(drawable.Shader);
        if (drawable.Culling.HasValue)
            previousCulling = graphics.SetCulling(drawable.Culling.Value);
        transform += drawable.Transform;
        graphics.PushMatrix();
        return new DrawScope<T>(
            originalTransform,
            drawable,
            graphics,
            previousBlendMode,
            previousShader,
            previousCulling
        );
    }

    public readonly record struct DrawScope<T>(
        in Transform Transform,
        T Drawable,
        Graphics Graphics,
        BlendMode? PreviousBlendMode = null,
        Shader? PreviousShader = null,
        bool? PreviousCulling = null
    ) : IDisposable
        where T : Drawable<T>
    {
        public void Dispose()
        {
            Graphics.PopMatrix();
            if (PreviousBlendMode.HasValue)
                Graphics.SetBlendMode(PreviousBlendMode.Value);
            if (PreviousShader is not null)
                Graphics.SetShader(PreviousShader);
            if (PreviousCulling.HasValue)
                Graphics.SetCulling(PreviousCulling.Value);
            Drawable.OnEndDrawing?.Invoke(Transform, Drawable, Graphics);
        }
    }
}

public abstract class Drawable<TSelf> : Drawable
    where TSelf : Drawable<TSelf>
{
    public Action<Transform, TSelf, Graphics>? OnBeginDrawing { get; set; } = null;
    public Action<Transform, TSelf, Graphics>? OnEndDrawing { get; set; } = null;
}
