using Vigilance.Core;

namespace Vigilance.Drawing;

public struct Sprite
{
    public Texture Texture = Texture.Empty;
    public bool FlipX = false;
    public bool FlipY = false;
    public Color Tint = Color.White;
    public Interpolation Interpolation = Game.DefaultInterpolation;
    public CameraProvider? Camera = Core.Camera.DefaultProvider;

    public Sprite() { }
}
