using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Grid : IFullCloneable
{
    public Grid() { }

    public Grid(float cellSize)
    {
        CellSize = cellSize;
    }

    public Grid(float cellSize, Color color)
        : this(cellSize)
    {
        Color = color;
    }

    public float CellSize { get; set; }
    public float Thick { get; set; } = Drawing.DefaultStrokeWidth == 0 ? 1 : Drawing.DefaultStrokeWidth;
    public Color Color { get; set; } = Drawing.DefaultFill;
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; } = 0;
    public Vector2 PivotPoint { get; set; } = Vector2.Zero;
    public Action<Transform, Grid, Graphics>? OnBeginDrawing { get; set; }
    public Action<Transform, Grid, Graphics>? OnEndDrawing { get; set; }

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
