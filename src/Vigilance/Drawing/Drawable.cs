using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public abstract class Drawable<T> : IDrawable
    where T : Drawable<T>
{
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; } = 0;
    public Vector2 PivotPoint { get; set; } = Vector2.Zero;
    public Action<Transform, T, Graphics>? OnBeginDrawing { get; set; }
    public Action<Transform, T, Graphics>? OnEndDrawing { get; set; }

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

    void IDrawable.Render(Transform transform, Graphics graphics)
    {
        Render(transform, graphics);
    }

    protected abstract void Render(Transform transform, Graphics graphics);
}
