using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class AnimationFrame
{
    public TimeSpan Delay { get; init; } = TimeSpan.Zero;
    public Texture? Texture { get; init; } = null;
    public bool? FlipX { get; init; } = null;
    public bool? FlipY { get; init; } = null;
    public Box? Source { get; init; } = null;
    public Color? Tint { get; init; } = null;
    public Interpolation? Interpolation { get; init; } = null;
}
