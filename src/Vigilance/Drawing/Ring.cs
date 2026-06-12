using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Ring : IFullCloneable
{
    public float InnerRadius { get; set; } = 0;
    public float OuterRadius { get; set; } = 0;
    public float StartAngle { get; set; } = 0;
    public float EndAngle { get; set; } = 360;
    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public int Segments { get; set; } = 0;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; } = 0;
    public Vector2 PivotPoint { get; set; } = Vector2.Zero;
    public Action<Transform, Ring, Graphics>? OnBeginDrawing { get; set; }
    public Action<Transform, Ring, Graphics>? OnEndDrawing { get; set; }

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

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform)), true);
    }
}
