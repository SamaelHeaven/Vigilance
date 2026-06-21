using System.Runtime.InteropServices;
using Vigilance.Math;

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
        get => _hasSource ? _source : null;
        set
        {
            _hasSource = value.HasValue;
            _source = value ?? default;
        }
    }

    public SpriteInstance() { }

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
}

public static class SpriteInstanceExtensions
{
    extension(SpriteAnimationFrame frame)
    {
        public void UpdateSpriteInstance(ref SpriteInstance sprite)
        {
            if (frame.FlipX.HasValue)
                sprite.FlipX = frame.FlipX.Value;
            if (frame.FlipY.HasValue)
                sprite.FlipY = frame.FlipY.Value;
            if (frame.Source.HasValue)
                sprite.Source = frame.Source;
            if (frame.Tint.HasValue)
                sprite.Tint = frame.Tint.Value;
            if (frame.Position.HasValue)
                sprite.Position = frame.Position.Value;
            if (frame.Scale.HasValue)
                sprite.Scale = frame.Scale.Value;
            if (frame.Rotation.HasValue)
                sprite.Rotation = frame.Rotation.Value;
            if (frame.PivotPoint.HasValue)
                sprite.PivotPoint = frame.PivotPoint.Value;
        }
    }
}
