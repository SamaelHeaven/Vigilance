using Vigilance.Core;

namespace Vigilance.Drawing;

public struct Sprite
{
    public Texture Texture = Texture.Empty;
    public bool FlippedHorizontally = false;
    public bool FlippedVertically = false;
    public Color Tint = Color.White;
    public Interpolation Interpolation = Game.DefaultInterpolation;
    public Func<Camera>? Camera = () => Game.Scene.Camera;

    public Sprite() { }
}
