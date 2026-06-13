using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Line : IFullCloneable
{
    public Line() { }

    public Line(Color color)
    {
        Color = color;
    }

    public Line(Vector2 start, Vector2 end)
    {
        Start = start;
        End = end;
    }

    public Line(Vector2 start, Vector2 end, Color color)
    {
        Start = start;
        End = end;
        Color = color;
    }

    public Vector2 Start { get; set; } = Vector2.Zero;
    public Vector2 End { get; set; } = Vector2.Zero;
    public Color Color { get; set; } = Drawing.DefaultFill;
    public float Thick { get; set; } = Drawing.DefaultStrokeWidth == 0 ? 1 : Drawing.DefaultStrokeWidth;
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; } = 0;
    public Vector2 PivotPoint { get; set; } = Vector2.Zero;
    public Action<Transform, Line, Graphics>? OnBeginDrawing { get; set; }
    public Action<Transform, Line, Graphics>? OnEndDrawing { get; set; }

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
