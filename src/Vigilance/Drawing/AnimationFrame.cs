using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed record AnimationFrame
{
    public TimeSpan Delay { get; init; } = TimeSpan.Zero;
    public Texture? Texture { get; init; } = null;
    public bool? FlipX { get; init; } = null;
    public bool? FlipY { get; init; } = null;
    public Wrapper<Box?>? Source { get; init; } = null;
    public Color? Tint { get; init; } = null;
    public Wrapper<NPatchInfo?>? NPatchInfo { get; set; } = null;
    public Interpolation? Interpolation { get; init; } = null;
}
