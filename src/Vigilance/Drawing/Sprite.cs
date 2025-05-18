using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Sprite
{
    public Texture Texture { get; set; } = Texture.Empty;
    public bool FlipX { get; set; } = false;
    public bool FlipY { get; set; } = false;
    public Box? Source { get; set; } = null;
    public Color Tint { get; set; } = Color.White;
    public Interpolation Interpolation { get; set; } = Game.DefaultInterpolation;
    public CameraProvider? Camera { get; set; } = Core.Camera.DefaultProvider;
}
