using Vigilance.Math;

namespace Vigilance.Drawing;

public struct AnimationFrame
{
    public TimeSpan Delay { get; set; } = TimeSpan.Zero;
    public Texture? Texture { get; set; } = null;
    public bool? FlipX { get; set; } = null;
    public bool? FlipY { get; set; } = null;
    public Box? Source { get; set; } = null;
    public Color? Tint { get; set; } = null;
    public Interpolation? Interpolation { get; set; } = null;

    public AnimationFrame() { }
}
