using Vigilance.Core;

namespace Vigilance.Drawing;

public struct Sprite
{
    public Texture Texture = Texture.Empty;
    public bool FlipX = false;
    public bool FlipY = false;
    public Color Tint = Color.White;
    public Filtering Filtering = Game.DefaultFiltering;
    public Func<Camera>? Camera = () => Game.Scene.Camera;

    public Sprite() { }
}
