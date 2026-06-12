using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Drawing;

public sealed class CustomPolygon : IFullCloneable
{
    public CustomPolygon() { }

    public CustomPolygon(IEnumerable<Vector2> points)
    {
        Points = points.ToList();
    }

    public CustomPolygon(IEnumerable<Vector2> points, Color fill)
        : this(points)
    {
        Fill = fill;
    }

    public List<Vector2> Points { get; set; } = [];
    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; } = 0;
    public Vector2 PivotPoint { get; set; } = Vector2.Zero;
    public Action<Transform, CustomPolygon, Graphics>? OnBeginDrawing { get; set; }
    public Action<Transform, CustomPolygon, Graphics>? OnEndDrawing { get; set; }

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

    object IDeepCloneable.DeepClone()
    {
        var result = this.ShallowClone();
        result.Points = Points.AsValueEnumerable().ToList();
        return result;
    }

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform)), true);
    }
}
