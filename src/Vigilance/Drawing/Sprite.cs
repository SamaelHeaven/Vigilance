using Vigilance.Core;

namespace Vigilance.Drawing;

public struct Sprite
{
    public Texture Texture { get; set; } = Texture.Empty;
    public bool FlipX { get; set; } = false;
    public bool FlipY { get; set; } = false;
    public Color Tint { get; set; } = Color.White;
    public Interpolation Interpolation { get; set; } = Game.DefaultInterpolation;
    public CameraProvider? Camera { get; set; } = Core.Camera.DefaultProvider;

    public Sprite() { }
}
