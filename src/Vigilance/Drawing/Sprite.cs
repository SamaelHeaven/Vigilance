using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Sprite : IFullCloneable
{
    public Sprite() { }

    public Sprite(Texture texture)
    {
        Texture = texture;
    }

    public Texture Texture { get; set; } = Texture.Empty;
    public bool FlipX { get; set; } = false;
    public bool FlipY { get; set; } = false;
    public Box? Source { get; set; } = null;
    public Color Tint { get; set; } = Color.White;
    public Interpolation Interpolation { get; set; } = Interpolation.Nearest;
    public CameraFunc? Camera { get; set; } = Core.Camera.Default;

    public override string ToString()
    {
        return Printer.Print(this);
    }
}
