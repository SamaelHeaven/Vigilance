using System.Runtime.InteropServices;

namespace Vigilance.Drawing;

[StructLayout(LayoutKind.Sequential)]
public record struct SpriteInstance
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; } = 0;
    public Vector2 PivotPoint { get; set; } = Vector2.Zero;
    public Color Tint { get; set; } = Color.White;
    public bool FlipX { get; set; } = false;
    public bool FlipY { get; set; } = false;
    private bool _hasSource = false;
    private Box _source = default;

    public Box? Source
    {
        readonly get => _hasSource ? _source : null;
        set
        {
            _hasSource = value.HasValue;
            _source = value ?? default;
        }
    }

    public Transform Transform
    {
        readonly get => new(Position, Scale, Rotation, PivotPoint);
        set
        {
            Position = value.Position;
            Scale = value.Scale;
            Rotation = value.Rotation;
            PivotPoint = value.PivotPoint;
        }
    }

    public SpriteInstance() { }
}
