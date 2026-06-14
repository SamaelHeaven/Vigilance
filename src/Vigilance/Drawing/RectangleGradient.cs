using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class RectangleGradient : IFullCloneable
{
    public Color TopLeftFill { get; set; } = Drawing.DefaultFill;
    public Color BottomLeftFill { get; set; } = Drawing.DefaultFill;
    public Color BottomRightFill { get; set; } = Drawing.DefaultFill;
    public Color TopRightFill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; } = 0;
    public Vector2 PivotPoint { get; set; } = Vector2.Zero;
    public Action<Transform, RectangleGradient, Graphics>? OnBeginDrawing { get; set; }
    public Action<Transform, RectangleGradient, Graphics>? OnEndDrawing { get; set; }

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

    public Color Fill
    {
        get => TopLeftFill.Blend(BottomLeftFill).Blend(BottomRightFill).Blend(TopRightFill);
        set
        {
            TopLeftFill = value;
            BottomLeftFill = value;
            BottomRightFill = value;
            TopRightFill = value;
        }
    }

    public Color TopFill
    {
        get => TopLeftFill.Blend(TopRightFill);
        set
        {
            TopLeftFill = value;
            TopRightFill = value;
        }
    }

    public Color BottomFill
    {
        get => BottomLeftFill.Blend(BottomRightFill);
        set
        {
            BottomLeftFill = value;
            BottomRightFill = value;
        }
    }

    public Color LeftFill
    {
        get => TopLeftFill.Blend(BottomLeftFill);
        set
        {
            TopLeftFill = value;
            BottomLeftFill = value;
        }
    }

    public Color RightFill
    {
        get => TopRightFill.Blend(BottomRightFill);
        set
        {
            TopRightFill = value;
            BottomRightFill = value;
        }
    }

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform), nameof(Fill)), true);
    }
}
