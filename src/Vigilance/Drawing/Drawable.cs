using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public abstract class Drawable<TSelf> : IDrawable, IFullCloneable
    where TSelf : Drawable<TSelf>
{
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; } = 0;
    public Vector2 PivotPoint { get; set; } = Vector2.Zero;
    public Action<Transform, TSelf, Graphics>? OnBeginDrawing { get; set; }
    public Action<Transform, TSelf, Graphics>? OnEndDrawing { get; set; }

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
